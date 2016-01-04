#region License
// Copyright (c) 2010-2016, Mark Final
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
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder :
        Bam.Core.IBuilderPreExecute
    {
        #region IBuilderPreExecute Members

        void
        Bam.Core.IBuilderPreExecute.PreExecute()
        {
#if true
#else
            Bam.Core.Log.DebugMessage("PreExecute for QMakeBuilder");

            var mainPackage = Bam.Core.State.PackageInfo[0];
            var priFileName = "EmptyConfig.pri";
            var rootDirectory = mainPackage.BuildDirectory;
            var priFilePath = System.IO.Path.Combine(rootDirectory, priFileName);

            System.IO.Directory.CreateDirectory(rootDirectory);
            using (System.IO.TextWriter proFileWriter = new System.IO.StreamWriter(priFilePath))
            {
                proFileWriter.WriteLine("# -- Generated by BuildAMation --");
                proFileWriter.WriteLine("TEMPLATE=");
                proFileWriter.WriteLine("# CONFIG cannot be completely emptied");
                proFileWriter.WriteLine("CONFIG-=qt");
                proFileWriter.WriteLine("CONFIG-=lex");
                proFileWriter.WriteLine("CONFIG-=yacc");
                proFileWriter.WriteLine("CONFIG-=warn_on");
                proFileWriter.WriteLine("CONFIG-=uic");
                proFileWriter.WriteLine("CONFIG-=resources");
                proFileWriter.WriteLine("CONFIG-=rtti_off");
                proFileWriter.WriteLine("CONFIG-=exceptions_off");
                proFileWriter.WriteLine("CONFIG-=stl_off");
                proFileWriter.WriteLine("CONFIG-=incremental_off");
                proFileWriter.WriteLine("CONFIG-=thread_off");
                proFileWriter.WriteLine("CONFIG-=windows");
                proFileWriter.WriteLine("CONFIG-=warn_on");
                proFileWriter.WriteLine("CONFIG-=incremental");
                proFileWriter.WriteLine("CONFIG-=flat");
                proFileWriter.WriteLine("CONFIG-=link_prl");
                proFileWriter.WriteLine("CONFIG-=precompile_header");
                proFileWriter.WriteLine("CONFIG-=autogen_precompile_source");
                proFileWriter.WriteLine("CONFIG-=copy_dir_files");
                proFileWriter.WriteLine("CONFIG-=embed_manifest_dll");
                proFileWriter.WriteLine("CONFIG-=embed_manifest_exe");
                proFileWriter.WriteLine("CONFIG-=shared");
                proFileWriter.WriteLine("CONFIG-=stl");
                proFileWriter.WriteLine("CONFIG-=exceptions");
                proFileWriter.WriteLine("CONFIG-=rtti");
                proFileWriter.WriteLine("CONFIG-=mmx");
                proFileWriter.WriteLine("CONFIG-=3dnow");
                proFileWriter.WriteLine("CONFIG-=sse");
                proFileWriter.WriteLine("CONFIG-=sse2");
                proFileWriter.WriteLine("CONFIG-=incredibuild_xge");
                proFileWriter.WriteLine("CONFIG-=console");
                proFileWriter.WriteLine("QT=");
                proFileWriter.WriteLine("DESTDIR=");
                proFileWriter.WriteLine("DEFINES=");
                proFileWriter.WriteLine("RC_FILE=");
                proFileWriter.WriteLine("QMAKE_COMPILER_DEFINES=");
                proFileWriter.WriteLine("QMAKE_CFLAGS=");
                proFileWriter.WriteLine("QMAKE_CFLAGS_DEBUG=");
                proFileWriter.WriteLine("QMAKE_CFLAGS_RELEASE=");
                proFileWriter.WriteLine("QMAKE_CXXFLAGS=");
                proFileWriter.WriteLine("QMAKE_CXXFLAGS_DEBUG=");
                proFileWriter.WriteLine("QMAKE_CXXFLAGS_RELEASE=");
                proFileWriter.WriteLine("QMAKE_LFLAGS=");
                proFileWriter.WriteLine("QMAKE_LFLAGS_DEBUG=");
                proFileWriter.WriteLine("QMAKE_LFLAGS_RELEASE=");
                proFileWriter.WriteLine("QMAKE_LN_SHLIB=");
                proFileWriter.WriteLine("QMAKE_MACOSX_DEPLOYMENT_TARGET=10.8");
            }

            this.EmptyConfigPriPath = priFilePath;
#endif
        }

        #endregion
    }
}
