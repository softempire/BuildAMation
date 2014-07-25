// <copyright file="CApplication.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object
        Build(
            C.Application moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Opus.Core.BaseTarget)target;

            var options = moduleToBuild.Options as C.LinkerOptionCollection;
            var executableLocation = moduleToBuild.Locations[C.Application.OutputFile];

            var project = this.Workspace.GetProject(node);

            var fileRef = project.FileReferences.Get(moduleName, PBXFileReference.EType.Executable, executableLocation, project.RootUri);
            project.ProductsGroup.Children.AddUnique(fileRef);

            var data = project.NativeTargets.Get(moduleName, PBXNativeTarget.EType.Executable, project);
            data.ProductReference = fileRef;

            // gather up all the source files for this target
            foreach (var childNode in node.Children)
            {
                if (childNode.Module is C.ObjectFileCollectionBase)
                {
                    foreach (var objectFile in childNode.Children)
                    {
                        var buildFile = objectFile.Data as PBXBuildFile;
                        data.SourceFilesToBuild.AddUnique(buildFile);
                    }
                }
                else
                {
                    var buildFile = childNode.Data as PBXBuildFile;
                    data.SourceFilesToBuild.AddUnique(buildFile);
                }
            }

            // build configuration target overrides to the project build configuration
            var buildConfiguration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), moduleName);
            var nativeTargetConfigurationList = project.ConfigurationLists.Get(data);
            nativeTargetConfigurationList.AddUnique(buildConfiguration);
            if (null == data.BuildConfigurationList)
            {
                data.BuildConfigurationList = nativeTargetConfigurationList;
            }
            else
            {
                if (data.BuildConfigurationList != nativeTargetConfigurationList)
                {
                    throw new Opus.Core.Exception("Inconsistent build configuration lists");
                }
            }

            // fill out the build configuration
            XcodeProjectProcessor.ToXcodeProject.Execute(moduleToBuild.Options, project, data, buildConfiguration, target);

            buildConfiguration.Options["PRODUCT_NAME"].AddUnique(options.OutputName);

            // Xcode 4 complains this is missing for target configurations
            buildConfiguration.Options["COMBINE_HIDPI_IMAGES"].AddUnique("YES");

            var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            var outputSuffix = linkerTool.ExecutableSuffix;
            buildConfiguration.Options["EXECUTABLE_SUFFIX"].AddUnique(outputSuffix);

            var basePath = Opus.Core.State.BuildRoot + System.IO.Path.DirectorySeparatorChar;
            var outputDirLoc = moduleToBuild.Locations[C.Application.OutputDir];
            var relPath = Opus.Core.RelativePathUtilities.GetPath(outputDirLoc, basePath);
            buildConfiguration.Options["CONFIGURATION_BUILD_DIR"].AddUnique("$SYMROOT/" + relPath);

            // adding the group for the target
            var group = project.Groups.Get(moduleName);
            group.SourceTree = "<group>";
            group.Path = moduleName;
            foreach (var source in node.Children)
            {
                if (source.Module is Opus.Core.IModuleCollection)
                {
                    foreach (var source2 in source.Children)
                    {
                        var sourceData = source2.Data as PBXBuildFile;
                        group.Children.AddUnique(sourceData.FileReference);
                    }
                }
                else
                {
                    var sourceData = source.Data as PBXBuildFile;
                    group.Children.AddUnique(sourceData.FileReference);
                }
            }
            data.Group = group;
            project.MainGroup.Children.AddUnique(group);

            var sourcesBuildPhase = project.SourceBuildPhases.Get("Sources", moduleName);
            data.BuildPhases.AddUnique(sourcesBuildPhase);

            var frameworksBuildPhase = project.FrameworksBuildPhases.Get("Frameworks", moduleName);
            data.BuildPhases.AddUnique(frameworksBuildPhase);

            if (null != node.ExternalDependents)
            {
                foreach (var dependency in node.ExternalDependents)
                {
                    var dependentData = dependency.Data as PBXNativeTarget;
                    if (null == dependentData)
                    {
                        continue;
                    }

                    // accumulate any scheme requirements from dependents
                    data.RequiredTargets.AddRangeUnique(dependentData.RequiredTargets);

                    if (dependentData.Project == project)
                    {
                        // first add a dependency so that they are built in the right order
                        // this is only required within the same project
                        var targetDependency = project.TargetDependencies.Get(moduleName, dependentData);

                        var containerItemProxy = project.ContainerItemProxies.Get(moduleName, dependentData, project);
                        targetDependency.TargetProxy = containerItemProxy;

                        data.Dependencies.Add(targetDependency);
                    }

                    // now add a link dependency
                    if (dependentData.Project == project)
                    {
                        var buildFile = project.BuildFiles.Get(dependency.UniqueModuleName, dependentData.ProductReference, frameworksBuildPhase);
                        if (null == buildFile)
                        {
                            throw new Opus.Core.Exception("Build file not available");
                        }

                        // now add linker search paths
                        if (dependency.Module is C.DynamicLibrary)
                        {
                            var outputDir = moduleToBuild.Locations[C.Application.OutputDir].GetSinglePath();
                            buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique(outputDir);
                        }
                        else if (dependency.Module is C.StaticLibrary)
                        {
                            var outputDir = moduleToBuild.Locations[C.StaticLibrary.OutputDirLocKey].GetSinglePath();
                            buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique(outputDir);
                        }
                    }
                    else
                    {
                        var type = dependentData.ProductReference.Type;
                        if (type == PBXFileReference.EType.StaticLibrary)
                        {
                            type = PBXFileReference.EType.ReferencedStaticLibrary;
                        }
                        if (type == PBXFileReference.EType.DynamicLibrary)
                        {
                            type = PBXFileReference.EType.ReferencedDynamicLibrary;
                        }

                        var relativePath = Opus.Core.RelativePathUtilities.GetPath(dependentData.ProductReference.FullPath, project.RootUri);
                        var dependentFileRef = project.FileReferences.Get(dependency.UniqueModuleName, type, relativePath, project.RootUri);
                        var buildFile = project.BuildFiles.Get(dependency.UniqueModuleName, dependentFileRef, frameworksBuildPhase);
                        if (null == buildFile)
                        {
                            throw new Opus.Core.Exception("Build file not available");
                        }

                        project.MainGroup.Children.AddUnique(dependentFileRef);

                        // now add linker search paths
                        buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique("$(inherited)");
                        if (dependency.Module is C.DynamicLibrary)
                        {
                            var outputDir = dependency.Module.Locations[C.Application.OutputDir].GetSinglePath();
                            buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique(outputDir);
                        }
                        else if (dependency.Module is C.StaticLibrary)
                        {
                            var outputDir = dependency.Module.Locations[C.StaticLibrary.OutputDirLocKey].GetSinglePath();
                            buildConfiguration.Options["LIBRARY_SEARCH_PATHS"].AddUnique(outputDir);
                        }
                    }
                }
            }

            // any required nodes must be registered as an ordering in the schema
            foreach (var req in node.EncapsulatingRequirements)
            {
                var reqData = req.Data as PBXNativeTarget;
                if (null != reqData)
                {
                    data.RequiredTargets.AddUnique(reqData);
                }
            }

            // find header files
            var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                        System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(fieldBindingFlags);
            foreach (var field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    var headerFileCollection = field.GetValue(moduleToBuild) as Opus.Core.FileCollection;
                    foreach (Opus.Core.Location location in headerFileCollection)
                    {
                        var headerPath = location.GetSinglePath();
                        var headerFileRef = project.FileReferences.Get(moduleName, PBXFileReference.EType.HeaderFile, headerPath, project.RootUri);
                        group.Children.AddUnique(headerFileRef);
                    }
                }
            }

            // TODO: this is the WRONG place to put this
            // add outstanding build phases made by nodes prior to this
            foreach (var scriptBuildPhase in project.ShellScriptBuildPhases)
            {
                data.BuildPhases.Insert(0, scriptBuildPhase as PBXShellScriptBuildPhase);
            }

            success = true;
            return data;
        }
    }
}
