#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace C
{
namespace V2
{
    using System.Linq;
    public sealed class MakeFileMeta
    {
        public MakeFileMeta(Bam.Core.V2.Module module)
        {
            this.Prequisities = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();
            this.Rules = new System.Collections.Generic.List<string>();

            if (Bam.Core.V2.Graph.Instance.IsReferencedModule(module))
            {
                this.TargetVariable = module.GetType().Name;
            }
        }

        public string Target
        {
            get;
            set;
        }

        public System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Prequisities
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<string> Rules
        {
            get;
            private set;
        }

        public string TargetVariable
        {
            get;
            private set;
        }

        public static void PreExecution()
        {
        }

        public static void PostExecution()
        {
            var graph = Bam.Core.V2.Graph.Instance;

            var command = new System.Text.StringBuilder();

            command.Append("all:");
            foreach (var module in graph.TopLevelModules)
            {
                var metadata = module.MetaData as MakeFileMeta;
                if (null == metadata)
                {
                    throw new Bam.Core.Exception("Top level module did not have any Make metadata");
                }
                command.AppendFormat("$({0}) ", metadata.TargetVariable);
            }
            command.AppendLine();

            foreach (var rank in graph.Reverse())
            {
                foreach (var module in rank)
                {
                    var metadata = module.MetaData as MakeFileMeta;
                    if (null == metadata)
                    {
                        continue;
                    }

                    if (metadata.TargetVariable != null)
                    {
                        command.AppendFormat("{0}:={1}", metadata.TargetVariable, metadata.Target);
                        command.AppendLine();
                        command.AppendFormat("$({0}):", metadata.TargetVariable);
                    }
                    else
                    {
                        command.AppendFormat("{0}:", metadata.Target);
                    }
                    foreach (var pre in metadata.Prequisities)
                    {
                        command.AppendFormat("{0} ", pre.Key.GeneratedPaths[pre.Value]);
                    }
                    command.AppendLine();
                    foreach (var rule in metadata.Rules)
                    {
                        command.AppendFormat("\t{0}", rule);
                        command.AppendLine();
                    }
                }
            }
            Bam.Core.Log.DebugMessage(command.ToString());
        }
    }

    public sealed class MakeFileCompilation :
        ICompilationPolicy
    {
        void
        ICompilationPolicy.Compile(
            ObjectFile sender,
            string objectFilePath,
            Bam.Core.V2.Module source)
        {
            var meta = new MakeFileMeta(sender);
            meta.Target = objectFilePath;
            meta.Prequisities.Add(source, C.V2.SourceFile.Key);
            meta.Rules.Add(sender.CommandLine.ToString(' '));
            sender.MetaData = meta;
        }
    }
}
}
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            C.ObjectFile moduleToBuild,
            out bool success)
        {
            var objectFileModule = moduleToBuild as Bam.Core.BaseModule;
            var node = objectFileModule.OwningNode;
            var target = node.Target;
            var moduleToolAttributes = moduleToBuild.GetType().GetCustomAttributes(typeof(Bam.Core.ModuleToolAssignmentAttribute), true);
            var toolType = (moduleToolAttributes[0] as Bam.Core.ModuleToolAssignmentAttribute).ToolType;
            var toolInterface = target.Toolset.Tool(toolType);
            var objectFileOptions = objectFileModule.Options;
            var compilerOptions = objectFileOptions as C.ICCompilerOptions;

            var sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();

            var inputFiles = new Bam.Core.StringArray();
            inputFiles.Add(sourceFilePath);

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);

            var commandLineBuilder = new Bam.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            var executable = toolInterface.Executable((Bam.Core.BaseTarget)target);

            string recipe = null;
            if (executable.Contains(" "))
            {
                recipe += System.String.Format("\"{0}\"", executable);
            }
            else
            {
                recipe += executable;
            }
            recipe += System.String.Format(" {0} $<", commandLineBuilder.ToString(' '));
            // replace target with $@
            var outputPath = moduleToBuild.Locations[C.ObjectFile.OutputFile].GetSinglePath();
            recipe = recipe.Replace(outputPath, "$@");

            var recipes = new Bam.Core.StringArray();
            recipes.Add(recipe);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                C.ObjectFile.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                null,
                inputFiles,
                recipes);

            var toolOutputLocKeys = toolInterface.OutputLocationKeys(moduleToBuild);
            var outputFileLocations = moduleToBuild.Locations.Keys(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
            var outputFileLocationsOfInterest = outputFileLocations.Intersect(toolOutputLocKeys);
            rule.OutputLocationKeys = outputFileLocationsOfInterest;

            makeFile.RuleArray.Add(rule);

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var targetDictionary = makeFile.ExportedTargets;
            var variableDictionary = makeFile.ExportedVariables;
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> environment = null;
            if (toolInterface is Bam.Core.IToolEnvironmentVariables)
            {
                environment = (toolInterface as Bam.Core.IToolEnvironmentVariables).Variables((Bam.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, targetDictionary, variableDictionary, environment);
            success = true;
            return returnData;
        }
    }
}
