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
namespace C
{
namespace V2
{
    public sealed class MakeFileCompilation :
        ICompilationPolicy
    {
        void
        ICompilationPolicy.Compile(
            ObjectFile sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString objectFilePath,
            Bam.Core.V2.Module source)
        {
            var commandLineArgs = new Bam.Core.StringArray();
            var interfaceType = Bam.Core.State.ScriptAssembly.GetType("CommandLineProcessor.V2.IConvertToCommandLine");
            if (interfaceType.IsAssignableFrom(sender.Settings.GetType()))
            {
                var map = sender.Settings.GetType().GetInterfaceMap(interfaceType);
                map.InterfaceMethods[0].Invoke(sender.Settings, new[] { sender, commandLineArgs as object });
            }

            var meta = new MakeFileBuilder.V2.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(objectFilePath);
            rule.AddPrerequisite(source, C.V2.SourceFile.Key);

            var tool = sender.Tool as Bam.Core.V2.PreBuiltTool;
            var command = new System.Text.StringBuilder();
            command.AppendFormat(tool.Executable.ContainsSpace ? "\"{0}\" {1}" : "{0} {1}", tool.Executable, commandLineArgs.ToString(' '));
            rule.AddShellCommand(command.ToString());

            var objectFileDir = System.IO.Path.GetDirectoryName(objectFilePath.ToString());
            meta.CommonMetaData.Directories.AddUnique(objectFileDir);
            meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);
        }
    }
}
}
