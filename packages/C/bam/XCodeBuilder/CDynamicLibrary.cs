#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object Build(C.DynamicLibrary moduleToBuild, out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var moduleName = node.ModuleName;
            var target = node.Target;
            var baseTarget = (Bam.Core.BaseTarget)target;

            var options = moduleToBuild.Options as C.LinkerOptionCollection;
            var executableLocation = moduleToBuild.Locations[C.Application.OutputFile];

            var project = this.Workspace.GetProject(node);

            // TODO: need to revisit Plugins once all builder generations are aligned for .bundle output

            //var fileType = (moduleToBuild is C.Plugin) ? PBXFileReference.EType.Plugin : PBXFileReference.EType.DynamicLibrary;
            var fileType = PBXFileReference.EType.DynamicLibrary;
            var fileRef = project.FileReferences.Get(moduleName, fileType, executableLocation, project.RootUri);
            project.ProductsGroup.Children.AddUnique(fileRef);

            //var targetType = (moduleToBuild is C.Plugin) ? PBXNativeTarget.EType.Plugin : PBXNativeTarget.EType.DynamicLibrary;
            var targetType = PBXNativeTarget.EType.DynamicLibrary;
            var data = project.NativeTargets.Get(moduleName, targetType, project);
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
                    throw new Bam.Core.Exception("Inconsistent build configuration lists");
                }
            }

            // fill out the build configuration
            XcodeProjectProcessor.ToXcodeProject.Execute(moduleToBuild.Options, project, data, buildConfiguration, target);

            buildConfiguration.Options["DYLIB_MAJOR_VERSION"].AddUnique((options as C.ILinkerOptions).MajorVersion.ToString());
            var productName = System.String.Format("{0}.$(DYLIB_MAJOR_VERSION)", options.OutputName);
            buildConfiguration.Options["PRODUCT_NAME"].AddUnique(productName);

            // Xcode 4 complains this is missing for target configurations
            buildConfiguration.Options["COMBINE_HIDPI_IMAGES"].AddUnique("YES");

            var linkerTool = target.Toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            var outputPrefix = linkerTool.DynamicLibraryPrefix;
            var outputSuffix = linkerTool.DynamicLibrarySuffix;
            buildConfiguration.Options["EXECUTABLE_PREFIX"].AddUnique(outputPrefix);
            buildConfiguration.Options["EXECUTABLE_SUFFIX"].AddUnique(outputSuffix);

            var basePath = Bam.Core.State.BuildRoot + System.IO.Path.DirectorySeparatorChar;
            var outputDirLoc = moduleToBuild.Locations[C.Application.OutputDir];
            var relPath = Bam.Core.RelativePathUtilities.GetPath(outputDirLoc, basePath);
            buildConfiguration.Options["CONFIGURATION_BUILD_DIR"].AddUnique("$SYMROOT/" + relPath);

            // adding the group for the target
            var group = project.Groups.Get(moduleName);
            group.SourceTree = "<group>";
            group.Path = moduleName;
            foreach (var source in node.Children)
            {
                if (source.Module is Bam.Core.IModuleCollection)
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
                            throw new Bam.Core.Exception("Build file not available");
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
                        if (type == PBXFileReference.EType.Plugin)
                        {
                            type = PBXFileReference.EType.ReferencedPlugin;
                        }

                        var relativePath = Bam.Core.RelativePathUtilities.GetPath(dependentData.ProductReference.FullPath, project.RootUri);
                        var dependentFileRef = project.FileReferences.Get(dependency.UniqueModuleName, type, relativePath, project.RootUri);
                        var buildFile = project.BuildFiles.Get(dependency.UniqueModuleName, dependentFileRef, frameworksBuildPhase);
                        if (null == buildFile)
                        {
                            throw new Bam.Core.Exception("Build file not available");
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
                    var headerFileCollection = field.GetValue(moduleToBuild) as Bam.Core.FileCollection;
                    foreach (Bam.Core.Location location in headerFileCollection)
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