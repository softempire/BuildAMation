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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
using C.V2.DefaultSettings;
namespace C
{
namespace V2
{
    public sealed partial class VSSolutionLinker :
        ILinkerPolicy
    {
        void
        ILinkerPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> frameworks)
        {
            if (0 == objectFiles.Count)
            {
                return;
            }

            var solution = Bam.Core.V2.Graph.Instance.MetaData as VSSolutionBuilder.V2.VSSolution;
            var project = solution.EnsureProjectExists(sender);
            var config = project.GetConfiguration(sender);

            config.SetType((sender is DynamicLibrary) ? VSSolutionBuilder.V2.VSProjectConfiguration.EType.DynamicLibrary : VSSolutionBuilder.V2.VSProjectConfiguration.EType.Application);
            config.SetPlatformToolset(VSSolutionBuilder.V2.VSProjectConfiguration.EPlatformToolset.v120); // TODO: get from VisualC
            config.SetOutputPath(executablePath);
            config.EnableIntermediatePath();

            foreach (var header in headers)
            {
                config.AddHeaderFile(header as HeaderFile);
            }

            var compilerGroup = config.GetSettingsGroup(VSSolutionBuilder.V2.VSSettingsGroup.ESettingsGroup.Compiler);
            if (objectFiles.Count > 1)
            {
                var vsConvertParameterTypes = new Bam.Core.TypeArray
                {
                    typeof(Bam.Core.V2.Module),
                    typeof(VSSolutionBuilder.V2.VSSettingsGroup),
                    typeof(string)
                };

                var sharedSettings = C.V2.SettingsBase.SharedSettings(
                    objectFiles,
                    typeof(VisualC.VSSolutionImplementation),
                    typeof(VisualStudioProcessor.V2.IConvertToProject),
                    vsConvertParameterTypes);
                (sharedSettings as VisualStudioProcessor.V2.IConvertToProject).Convert(sender, compilerGroup);

                foreach (var objFile in objectFiles)
                {
                    var deltaSettings = (objFile.Settings as C.V2.SettingsBase).CreateDeltaSettings(sharedSettings, objFile);
                    config.AddSourceFile(objFile, deltaSettings);
                }
            }
            else
            {
                (objectFiles[0].Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(sender, compilerGroup);
                foreach (var objFile in objectFiles)
                {
                    config.AddSourceFile(objFile, null);
                }
            }

            foreach (var input in libraries)
            {
                if (null != input.MetaData)
                {
                    if ((input is C.V2.StaticLibrary) || (input is C.V2.DynamicLibrary))
                    {
                        project.LinkAgainstProject(solution.EnsureProjectExists(input));
                    }
                    else if ((input is C.V2.CSDKModule) || (input is C.V2.HeaderLibrary))
                    {
                        continue;
                    }
                    else if (input is ExternalFramework)
                    {
                        throw new Bam.Core.Exception("Frameworks are not supported on Windows: {0}", input.ToString());
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Don't know how to handle this buildable library module, {0}", input.ToString());
                    }
                }
                else
                {
                    if (input is C.V2.StaticLibrary)
                    {
                        // TODO: probably a simplification of the DLL codepath
                        throw new System.NotImplementedException();
                    }
                    else if (input is C.V2.DynamicLibrary)
                    {
                        // TODO: this might be able to shift out of the conditional
                        (sender.Tool as C.V2.LinkerTool).ProcessLibraryDependency(sender as CModule, input as CModule);
                    }
                    else if ((input is C.V2.CSDKModule) || (input is C.V2.HeaderLibrary))
                    {
                        continue;
                    }
                    else if (input is ExternalFramework)
                    {
                        throw new Bam.Core.Exception("Frameworks are not supported on Windows: {0}", input.ToString());
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Don't know how to handle this prebuilt library module, {0}", input.ToString());
                    }
                }
            }

            var linkerGroup = config.GetSettingsGroup(VSSolutionBuilder.V2.VSSettingsGroup.ESettingsGroup.Linker);
            (sender.Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(sender, linkerGroup);

            // order only dependencies
            foreach (var required in sender.Requirements)
            {
                if (null == required.MetaData)
                {
                    continue;
                }

                var requiredProject = required.MetaData as VSSolutionBuilder.V2.VSProject;
                if (null != requiredProject)
                {
                    project.RequiresProject(requiredProject);
                }
            }
        }
    }
}
}
