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
namespace OpenGLSDK
{
    // TODO: this is pretty pointless at the moment
    public sealed class OpenGLV2 :
        C.V2.CSDKModule
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);

            this.PublicPatch((settings, appliedTo) =>
                {
                    var linker = settings as C.V2.ICommonLinkerOptions;
                    if (null != linker)
                    {
                        if (linker is VisualC.V2.LinkerSettings)
                        {
                            linker.Libraries.Add("OPENGL32.lib");
                        }
                            /*
                        else if (linker is Mingw.V2.LinkerBase)
                        {
                            linker.Libraries.Add("-lopengl32");
                        }
                             */
                    }
                    var osxLinker = settings as C.V2.ILinkerOptionsOSX;
                    if (null != osxLinker)
                    {
                        osxLinker.Frameworks.AddUnique(Bam.Core.V2.TokenizedString.Create("OpenGL", null, verbatim: true));
                    }
                });
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
        }

        protected override void ExecuteInternal(Bam.Core.V2.ExecutionContext context)
        {
            // do nothing
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }

    class OpenGL :
        C.ThirdPartyModule
    {
        class TargetFilter :
            Bam.Core.BaseTargetFilteredAttribute
        {}

        private static readonly TargetFilter winVCTarget;
        private static readonly TargetFilter winMingwTarget;
        private static readonly TargetFilter unixTarget;
        private static readonly TargetFilter osxTarget;

        static
        OpenGL()
        {
            winVCTarget = new TargetFilter();
            winVCTarget.Platform = Bam.Core.EPlatform.Windows;
            winVCTarget.ToolsetTypes = new[] { typeof(VisualC.Toolset) };

            winMingwTarget = new TargetFilter();
            winMingwTarget.Platform = Bam.Core.EPlatform.Windows;
            winMingwTarget.ToolsetTypes = new[] { typeof(Mingw.Toolset) };

            unixTarget = new TargetFilter();
            unixTarget.Platform = Bam.Core.EPlatform.Unix;

            osxTarget = new TargetFilter();
            osxTarget.Platform = Bam.Core.EPlatform.OSX;
        }

        public
        OpenGL()
        {
            this.UpdateOptions += new Bam.Core.UpdateOptionCollectionDelegate(OpenGL_LinkerOptions);
        }

        [C.ExportLinkerOptionsDelegate]
        void
        OpenGL_LinkerOptions(
            Bam.Core.IModule module,
            Bam.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (null == linkerOptions)
            {
                return;
            }

            // add libraries
            var libraries = new Bam.Core.StringArray();
            if (Bam.Core.TargetUtilities.MatchFilters(target, winVCTarget))
            {
                libraries.Add(@"OPENGL32.lib");
            }
            else if (Bam.Core.TargetUtilities.MatchFilters(target, winMingwTarget))
            {
                libraries.Add("-lopengl32");
            }
            else if (Bam.Core.TargetUtilities.MatchFilters(target, unixTarget))
            {
                libraries.Add("-lGL");
            }
            else if (Bam.Core.TargetUtilities.MatchFilters(target, osxTarget))
            {
                var osxLinkerOptions = module.Options as C.ILinkerOptionsOSX;
                if (null != osxLinkerOptions)
                {
                    osxLinkerOptions.Frameworks.Add("OpenGL");
                }
            }
            else
            {
                throw new Bam.Core.Exception("Unsupported OpenGL platform");
            }

            if (libraries.Count > 0)
            {
                linkerOptions.Libraries.AddRange (libraries);
            }
        }

        [Bam.Core.DependentModules(Platform = Bam.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Bam.Core.TypeArray winVCDependentModules = new Bam.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }
}
