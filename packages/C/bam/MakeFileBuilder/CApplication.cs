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
namespace C
{
namespace V2
{
    public sealed class MakeFileLinker :
        ILinkerPolicy
    {
        private static string
        GetLibraryPath(Bam.Core.V2.Module module)
        {
            if (module is C.V2.StaticLibrary)
            {
                return module.GeneratedPaths[C.V2.StaticLibrary.Key].ToString();
            }
            else if (module is C.V2.DynamicLibrary)
            {
                if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    return module.GeneratedPaths[C.V2.DynamicLibrary.ImportLibraryKey].ToString();
                }
                else
                {
                    return module.GeneratedPaths[C.V2.DynamicLibrary.Key].ToString();
                }
            }
            else if (module is C.V2.CSDKModule)
            {
                // collection of libraries, none in particular
                return null;
            }
            else if (module is C.V2.HeaderLibrary)
            {
                // no library
                return null;
            }
            else if (module is ExternalFramework)
            {
                // dealt with elsewhere
                return null;
            }
            else
            {
                throw new Bam.Core.Exception("Unknown module library type: {0}", module.GetType());
            }
        }

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
            // TODO: modify to use ProcessLibraryDependency
            var linker = sender.Settings as C.V2.ICommonLinkerOptions;
            // TODO: could the lib search paths be in the staticlibrary base class as a patch?
            foreach (var library in libraries)
            {
                var fullLibraryPath = GetLibraryPath(library);
                if (null == fullLibraryPath)
                {
                    continue;
                }
                var dir = System.IO.Path.GetDirectoryName(fullLibraryPath);
                linker.LibraryPaths.AddUnique(Bam.Core.V2.TokenizedString.Create(dir, null));
            }

            var commandLineArgs = new Bam.Core.StringArray();
            var interfaceType = Bam.Core.State.ScriptAssembly.GetType("CommandLineProcessor.V2.IConvertToCommandLine");
            if (interfaceType.IsAssignableFrom(sender.Settings.GetType()))
            {
                var map = sender.Settings.GetType().GetInterfaceMap(interfaceType);
                map.InterfaceMethods[0].Invoke(sender.Settings, new[] { sender, commandLineArgs as object });
            }

            var meta = new MakeFileBuilder.V2.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(executablePath);
            foreach (var module in objectFiles)
            {
                rule.AddPrerequisite(module, C.V2.ObjectFile.Key);
            }
            foreach (var module in libraries)
            {
                if (module is StaticLibrary)
                {
                    rule.AddPrerequisite(module, C.V2.StaticLibrary.Key);
                }
                else if (module is DynamicLibrary)
                {
                    if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        rule.AddPrerequisite(module, C.V2.DynamicLibrary.ImportLibraryKey);
                    }
                    else
                    {
                        rule.AddPrerequisite(module, C.V2.DynamicLibrary.Key);
                    }
                }
                else if (module is CSDKModule)
                {
                    continue;
                }
                else if (module is ExternalFramework)
                {
                    continue;
                }
                else
                {
                    throw new Bam.Core.Exception("Unknown module library type: {0}", module.GetType());
                }
            }

            var tool = sender.Tool as Bam.Core.V2.PreBuiltTool;
            var commands = new System.Text.StringBuilder();
            commands.AppendFormat(tool.Executable.ContainsSpace ? "\"{0}\" $^ {1}" : "{0} $^ {1}", tool.Executable, commandLineArgs.ToString(' '));
            rule.AddShellCommand(commands.ToString());

            var executableDir = System.IO.Path.GetDirectoryName(executablePath.ToString());
            meta.CommonMetaData.Directories.AddUnique(executableDir);
            meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);
        }
    }
}
}
