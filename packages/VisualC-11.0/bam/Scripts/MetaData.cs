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
namespace VisualC
{
    public class MetaData :
        Bam.Core.PackageMetaData,
        VisualCCommon.IRuntimeLibraryPathMeta
    {
        private System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string,object>();

        public MetaData()
        {
            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            // TODO: get this from the registry
            this.InstallDir = Bam.Core.TokenizedString.Create("$(0)/Microsoft Visual Studio 11.0", null, new Bam.Core.TokenizedStringArray(Bam.Core.OSUtilities.WindowsProgramFilesx86Path));
            if (!System.IO.Directory.Exists(this.InstallDir.Parse()))
            {
                throw new Bam.Core.Exception("'{0}' was not found. Was VisualStudio 2012 installed?", this.InstallDir.Parse());
            }
            this.PlatformToolset = "v110";
            this.VCXProjToolsVersion = "4.0";
            this.VCXProjFiltersToolsVersion = "4.0";
            this.UseWindowsSDKPublicPatches = false;
        }

        public override object this[string index]
        {
            get
            {
                return this.Meta[index];
            }
        }

        public override bool
        Contains(
            string index)
        {
            return this.Meta.ContainsKey(index);
        }

        public Bam.Core.TokenizedString
        InstallDir
        {
            get
            {
                return this.Meta["InstallDir"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["InstallDir"] = value;
            }
        }

        public string
        PlatformToolset
        {
            get
            {
                return this.Meta["PlatformToolset"] as string;
            }

            private set
            {
                this.Meta["PlatformToolset"] = value;
            }
        }

        public string
        VCXProjToolsVersion
        {
            get
            {
                return this.Meta["VCXProjToolsVersion"] as string;
            }

            private set
            {
                this.Meta["VCXProjToolsVersion"] = value;
            }
        }

        public string
        VCXProjFiltersToolsVersion
        {
            get
            {
                return this.Meta["VCXProjFiltersToolsVersion"] as string;
            }

            private set
            {
                this.Meta["VCXProjFiltersToolsVersion"] = value;
            }
        }

        public bool
        UseWindowsSDKPublicPatches
        {
            get
            {
                return (bool)this.Meta["RequiresWindowsSDK"];
            }

            private set
            {
                this.Meta["RequiresWindowsSDK"] = value;
            }
        }

        Bam.Core.TokenizedStringArray
        VisualCCommon.IRuntimeLibraryPathMeta.CRuntimePaths(
            C.EBit depth)
        {
            var dynamicLibPaths = new Bam.Core.TokenizedStringArray();
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/redist/x86/Microsoft.VC110.CRT/msvcr110.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir)));
                    break;

                case C.EBit.SixtyFour:
                    dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/redist/x64/Microsoft.VC110.CRT/msvcr110.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir)));
                    break;

                default:
                    throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
            }
            return dynamicLibPaths;
        }

        Bam.Core.TokenizedStringArray
        VisualCCommon.IRuntimeLibraryPathMeta.CxxRuntimePaths(
            C.EBit depth)
        {
            var dynamicLibPaths = new Bam.Core.TokenizedStringArray();
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/redist/x86/Microsoft.VC110.CRT/msvcp110.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir)));
                    break;

                case C.EBit.SixtyFour:
                    dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/redist/x64/Microsoft.VC110.CRT/msvcp110.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir)));
                    break;

                default:
                    throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
            }
            return dynamicLibPaths;
        }
    }
}
