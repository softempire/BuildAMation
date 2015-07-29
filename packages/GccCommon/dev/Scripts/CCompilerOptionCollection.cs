#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace GccCommon
{
namespace V2
{
    namespace DefaultSettings
    {
        public static partial class DefaultSettingsExtensions
        {
            public static void
            Defaults(this GccCommon.V2.ICommonCompilerOptions settings, Bam.Core.V2.Module module)
            {
                settings.PositionIndependentCode = false;
            }

            public static void
            Defaults(this GccCommon.V2.ICommonLinkerOptions settings, Bam.Core.V2.Module module)
            {
                settings.CanUseOrigin = false;
                settings.RPath = new Bam.Core.StringArray();
            }
        }
    }

    public static partial class NativeImplementation
    {
        public static void
        Convert(
            this GccCommon.V2.ICommonCompilerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (null != options.PositionIndependentCode)
            {
                if (true == options.PositionIndependentCode)
                {
                    commandLine.Add("-fPIC");
                }
            }
        }

        public static void
        Convert(
            this GccCommon.V2.ICommonLinkerOptions options,
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commandLine)
        {
            if (null != options.CanUseOrigin)
            {
                if (true == options.CanUseOrigin)
                {
                    commandLine.Add("-Wl,-z,origin");
                }
            }
            foreach (var rpath in options.RPath)
            {
                commandLine.Add(System.String.Format("-Wl,-rpath,{0}", rpath));
            }
        }
    }
}
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        C.CompilerOptionCollection,
        C.ICCompilerOptions,
        ICCompilerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            var localCompilerOptions = this as ICCompilerOptions;
            localCompilerOptions.AllWarnings = true;
            localCompilerOptions.ExtraWarnings = true;

            base.SetDefaultOptionValues(node);

            var target = node.Target;
            localCompilerOptions.SixtyFourBit = Bam.Core.OSUtilities.Is64Bit(target);

            if (target.HasConfiguration(Bam.Core.EConfiguration.Debug))
            {
                localCompilerOptions.StrictAliasing = false;
                localCompilerOptions.InlineFunctions = false;
            }
            else
            {
                localCompilerOptions.StrictAliasing = true;
                localCompilerOptions.InlineFunctions = true;
            }

            localCompilerOptions.PositionIndependentCode = false;

            var toolset = target.Toolset;
            var compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            var cCompilerOptions = this as C.ICCompilerOptions;
            cCompilerOptions.SystemIncludePaths.AddRange(compilerTool.IncludePaths((Bam.Core.BaseTarget)node.Target));

            cCompilerOptions.TargetLanguage = C.ETargetLanguage.C;

            localCompilerOptions.Pedantic = true;

            if (null != node.Children)
            {
                // we use gcc as the compile - if there is ObjectiveC code included, disable ignoring standard include paths as it gets complicated otherwise
                foreach (var child in node.Children)
                {
                    if (child.Module is C.ObjC.ObjectFile || child.Module is C.ObjC.ObjectFileCollection ||
                        child.Module is C.ObjCxx.ObjectFile || child.Module is C.ObjCxx.ObjectFileCollection)
                    {
                        cCompilerOptions.IgnoreStandardIncludePaths = false;
                        break;
                    }
                }
            }
        }

        public
        CCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
