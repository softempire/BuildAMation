// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(FileUtilities.CopyFiles copyFiles, out bool success)
        {
            System.Enum sourceOutputPaths = copyFiles.SourceOutputFlags;
            System.Collections.Generic.List<MakeFileData> sourceFileDataArray = new System.Collections.Generic.List<MakeFileData>();
            Opus.Core.StringArray sourceFiles = null;
            foreach (Opus.Core.IModule sourceModule in copyFiles.SourceModules)
            {
                if (sourceModule.Options.OutputPaths.Has(sourceOutputPaths))
                {
                    if (sourceModule.OwningNode.Data != null)
                    {
                        MakeFileData data = sourceModule.OwningNode.Data as MakeFileData;
                        sourceFileDataArray.Add(data);
                    }
                }
            }
            if (null != copyFiles.SourceFiles)
            {
                sourceFiles = new Opus.Core.StringArray();
                foreach (string path in copyFiles.SourceFiles)
                {
                    sourceFiles.Add(path);
                }
            }
            if (0 == sourceFileDataArray.Count && null == sourceFiles)
            {
                Opus.Core.Log.DebugMessage("No files to copy");
                success = true;
                return null;
            }

            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
            string destinationDirectory = null;
            System.Enum destinationOutputFlags;
            if (null != destinationModule)
            {
                Opus.Core.StringArray destinationPaths = new Opus.Core.StringArray();
                destinationModule.Options.FilterOutputPaths(copyFiles.DirectoryOutputFlags, destinationPaths);
                destinationOutputFlags = copyFiles.DirectoryOutputFlags;
                destinationDirectory = System.IO.Path.GetDirectoryName(destinationPaths[0]);
            }
            else
            {
                destinationOutputFlags = sourceOutputPaths;
                destinationDirectory = copyFiles.DestinationDirectory;
            }

            Opus.Core.IModule copyFilesModule = copyFiles as Opus.Core.IModule;
            Opus.Core.DependencyNode node = copyFilesModule.OwningNode;
            Opus.Core.Target target = node.Target;
            // NEW STYLE
#if true
            Opus.Core.IToolset toolset = target.Toolset;
            Opus.Core.ITool tool = toolset.Tool(typeof(FileUtilities.ICopyFilesTool));
#else
            FileUtilities.CopyFilesTool tool = new FileUtilities.CopyFilesTool();
#endif
            string toolExecutablePath = tool.Executable(target);

            Opus.Core.BaseOptionCollection copyFilesOptions = copyFilesModule.Options;

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (copyFilesOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = copyFilesOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            string recipe = null;
            if (toolExecutablePath.Contains(" "))
            {
                recipe += System.String.Format("\"{0}\"", toolExecutablePath);
            }
            else
            {
                recipe += toolExecutablePath;
            }
            recipe += System.String.Format(" {0} $< $@", commandLineBuilder.ToString(' '));
            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();
            directoriesToCreate.AddAbsoluteDirectory(destinationDirectory, false);

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Opus.Core.Log.DebugMessage("Makefile path : '{0}'", makeFilePath);

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            Opus.Core.StringArray destinationPathNames = new Opus.Core.StringArray();
            foreach (MakeFileData data in sourceFileDataArray)
            {
                Opus.Core.StringArray variableDictionary = data.VariableDictionary[sourceOutputPaths];
                foreach (string variable in variableDictionary)
                {
                    string destinationPathName = System.String.Format("{0}{1}$(notdir $({2}))", destinationDirectory, System.IO.Path.DirectorySeparatorChar, variable);
                    if (destinationPathNames.Contains(destinationPathName))
                    {
                        Opus.Core.Log.DebugMessage("Target pathname '{0}' has already been defined", destinationPathName);
                        continue;
                    }

                    Opus.Core.OutputPaths outputPaths = new Opus.Core.OutputPaths();
                    outputPaths[destinationOutputFlags] = destinationPathName;

                    MakeFileVariableDictionary singleEntry = new MakeFileVariableDictionary();
                    singleEntry.Add(sourceOutputPaths, new Opus.Core.StringArray(variable));

                    MakeFileRule rule = new MakeFileRule(outputPaths, destinationOutputFlags, node.UniqueModuleName, directoriesToCreate, singleEntry, null, recipes);
                    makeFile.RuleArray.Add(rule);

                    destinationPathNames.Add(destinationPathName);
                }
            }
            if (null != sourceFiles)
            {
                foreach (string sourceFile in sourceFiles)
                {
                    string destinationPathName = System.String.Format("{0}{1}$(notdir {2})", destinationDirectory, System.IO.Path.DirectorySeparatorChar, sourceFile);

                    Opus.Core.OutputPaths outputPaths = new Opus.Core.OutputPaths();
                    outputPaths[destinationOutputFlags] = destinationPathName;

                    MakeFileRule rule = new MakeFileRule(outputPaths, destinationOutputFlags, node.UniqueModuleName, directoriesToCreate, null, new Opus.Core.StringArray(sourceFile), recipes);
                    makeFile.RuleArray.Add(rule);
                }
            }

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            success = true;

            MakeFileData nodeData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null);
            return nodeData;
        }
    }
}
