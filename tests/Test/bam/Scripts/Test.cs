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
using Bam.Core.V2;
namespace Test
{
    public sealed class LocalPolicy :
        Bam.Core.V2.ISitePolicy
    {
        void
        ISitePolicy.DefineLocalSettings(
            Settings settings,
            Module module)
        {
            var compiler = settings as C.V2.ICommonCompilerOptions;
            if (null != compiler)
            {
                compiler.PreprocessorDefines.Add("GLOBALOVERRIDE");
            }

            var cxxCompiler = settings as C.V2.ICxxOnlyCompilerOptions;
            if (null != cxxCompiler)
            {
                cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            }
        }
    }

    sealed class CompileSingleCFileV2 :
        C.V2.ObjectFile
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.InputPath = Bam.Core.V2.TokenizedString.Create("$(pkgroot)/source/main.c", this);
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                // example of switching out the tool within a module
                this.Compiler = Bam.Core.V2.Graph.Instance.FindReferencedModule<Mingw.V2.Compiler32>();
            }
        }
    }

    sealed class CompileSingleCFileWithCustomOptionsV2 :
        C.V2.ObjectFile
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.InputPath = Bam.Core.V2.TokenizedString.Create("$(pkgroot)/source/main.c", this);
            this.PrivatePatch(settings =>
            {
                var compiler = settings as C.V2.ICommonCompilerOptions;
                compiler.DebugSymbols = false;
            });
        }
    }

    sealed class BuildTerminalApplicationFromCV2 :
        C.V2.ConsoleApplication
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.CreateCSourceContainer("$(pkgroot)/source/main.c");

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    sealed class BuildTerminalApplicationFromCxxV2 :
        C.Cxx.V2.ConsoleApplication
    {
        protected override void Init(Module parent)
        {
            base.Init(parent);

            this.CreateCxxSourceContainer("$(pkgroot)/source/main.c");

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDKV2>();
            }
        }
    }

    [Bam.Core.V2.PlatformFilter(Bam.Core.EPlatform.Windows)]
    sealed class BuildWindowsApplicationV2 :
        C.V2.GUIApplication
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var source = this.CreateCSourceContainer("$(pkgroot)/source/main.c");
            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.V2.ICommonCompilerOptions;
                    compiler.WarningsAsErrors = false;
                });

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualC.V2.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDKV2>(source);
            }
        }
    }
}
