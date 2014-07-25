// <copyright file="CApplication.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            C.Application moduleToBuild,
            out bool success)
        {
            var applicationModule = moduleToBuild as Opus.Core.BaseModule;
            var node = applicationModule.OwningNode;
            var target = node.Target;

            // dependents
            var inputVariables = new MakeFileVariableDictionary();
            var dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (null != node.Children)
            {
                var keysToFilter = new Opus.Core.Array<Opus.Core.LocationKey>(
                    C.ObjectFile.OutputFile
                );

                foreach (var childNode in node.Children)
                {
                    if (null == childNode.Data)
                    {
                        continue;
                    }
                    var data = childNode.Data as MakeFileData;
                    inputVariables.Append(data.VariableDictionary.Filter(keysToFilter));
                    dataArray.Add(data);
                }
            }
            if (null != node.ExternalDependents)
            {
                var libraryKeysToFilter = new Opus.Core.Array<Opus.Core.LocationKey>(
                    C.StaticLibrary.OutputFileLocKey);
                if (target.HasPlatform(Opus.Core.EPlatform.Unix))
                {
                    // TODO: why is the symlink not present?
                    //libraryKeysToFilter.Add(C.PosixSharedLibrarySymlinks.LinkerSymlink);
                    libraryKeysToFilter.Add(C.DynamicLibrary.OutputFile);
                }
                else if (target.HasPlatform(Opus.Core.EPlatform.Windows))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.ImportLibraryFile);
                }
                else if (target.HasPlatform(Opus.Core.EPlatform.OSX))
                {
                    libraryKeysToFilter.Add(C.DynamicLibrary.OutputFile);
                }

                foreach (var dependentNode in node.ExternalDependents)
                {
                    if (null == dependentNode.Data)
                    {
                        continue;
                    }
                    var data = dependentNode.Data as MakeFileData;
                    inputVariables.Append(data.VariableDictionary.Filter(libraryKeysToFilter));
                    dataArray.Add(data);
                }
            }

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);

            var applicationOptions = applicationModule.Options;
            var commandLineBuilder = new Opus.Core.StringArray();
            if (applicationOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = applicationOptions as CommandLineProcessor.ICommandLineSupport;
                if (target.HasPlatform(Opus.Core.EPlatform.Windows))
                {
                    // libraries are manually added later
                    var excludedOptions = new Opus.Core.StringArray("Libraries", "StandardLibraries");
                    commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptions);
                }
                else
                {
                    var excludedOptions = new Opus.Core.StringArray();
                    excludedOptions.Add("RPath"); // $ORIGIN is handled differently
                    excludedOptions.Add("Libraries");
                    excludedOptions.Add("StandardLibraries");
                    commandLineOption.ToCommandLineArguments(commandLineBuilder, target, excludedOptions);

                    // handle RPath separately
                    // what needs to happen is that the $ must be doubled, and the entire path quoted
                    // http://stackoverflow.com/questions/230364/how-to-get-rpath-with-origin-to-work-on-codeblocks-gcc
                    var rPathCommandLine = new Opus.Core.StringArray();
                    var optionNames = new Opus.Core.StringArray();
                    optionNames.Add("RPath");
                    CommandLineProcessor.ToCommandLine.ExecuteForOptionNames(applicationOptions, rPathCommandLine, target, optionNames);
                    foreach (var rpath in rPathCommandLine)
                    {
                        var linkerCommand = rpath.Split(',');
                        var rpathDir = linkerCommand[linkerCommand.Length - 1];
                        rpathDir = rpathDir.Replace("$ORIGIN", "$$ORIGIN");
                        rpathDir = System.String.Format("'{0}'", rpathDir);
                        var linkerCommandArray = new Opus.Core.StringArray(linkerCommand);
                        linkerCommandArray.Remove(linkerCommand[linkerCommand.Length - 1]);
                        linkerCommandArray.Add(rpathDir);
                        var newRPathCommand = linkerCommandArray.ToString(',');
                        commandLineBuilder.Add(newRPathCommand);
                    }
                }
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            var toolset = target.Toolset;
            var linkerTool = toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            var executable = linkerTool.Executable((Opus.Core.BaseTarget)target);

            var recipeBuilder = new System.Text.StringBuilder();
            if (executable.Contains(" "))
            {
                recipeBuilder.AppendFormat("\"{0}\"", executable);
            }
            else
            {
                recipeBuilder.Append(executable);
            }

            var dependentLibraryCommandLine = new Opus.Core.StringArray();
            {
                recipeBuilder.AppendFormat(" {0} ", commandLineBuilder.ToString(' '));

                var dependentLibraries = new Opus.Core.StringArray("$^");
                C.LinkerUtilities.AppendLibrariesToCommandLine(dependentLibraryCommandLine, linkerTool, applicationOptions as C.ILinkerOptions, dependentLibraries);
            }

            recipeBuilder.Append(dependentLibraryCommandLine.ToString(' '));
            var recipe = recipeBuilder.ToString();
            // replace primary target with $@
            var primaryOutputKey = C.Application.OutputFile;
            var outputPath = moduleToBuild.Locations[primaryOutputKey].GetSinglePath();
            recipe = recipe.Replace(outputPath, "$@");
            var instanceName = MakeFile.InstanceName(node);
            var toolOutputLocKeys = (linkerTool as Opus.Core.ITool).OutputLocationKeys(moduleToBuild);
            var outputFileLocations = moduleToBuild.Locations.Keys(Opus.Core.ScaffoldLocation.ETypeHint.File, Opus.Core.Location.EExists.WillExist);
            var outputFileLocationsOfInterest = outputFileLocations.Intersect(toolOutputLocKeys);

            // replace non-primary outputs with their variable names
            foreach (var outputKey in outputFileLocations)
            {
                if (outputKey == primaryOutputKey)
                {
                    continue;
                }

                var outputLoc = moduleToBuild.Locations[outputKey];
                if (!outputLoc.IsValid)
                {
                    continue;
                }

                var variableName = System.String.Format("$({0}_{1}_Variable)", instanceName, outputKey.ToString());
                var outputLocPath = outputLoc.GetSinglePath();
                recipe = recipe.Replace(outputLocPath, variableName);
            }

            var recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                C.Application.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                inputVariables,
                null,
                recipes);
            rule.OutputLocationKeys = outputFileLocationsOfInterest;
            makeFile.RuleArray.Add(rule);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var exportedTargets = makeFile.ExportedTargets;
            var exportedVariables = makeFile.ExportedVariables;
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment = null;
            if (linkerTool is Opus.Core.IToolEnvironmentVariables)
            {
                environment = (linkerTool as Opus.Core.IToolEnvironmentVariables).Variables((Opus.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, exportedTargets, exportedVariables, environment);
            success = true;
            return returnData;
        }
    }
}
