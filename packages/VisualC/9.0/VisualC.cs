// <copyright file="VisualC.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("C", "visualc", "VisualC.Toolchain.VersionString")]

[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.ArchiverTool, typeof(VisualCCommon.Archiver), typeof(VisualC.ArchiverOptionCollection))]
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.CCompilerTool, typeof(VisualCCommon.CCompiler), typeof(VisualC.CCompilerOptionCollection))]
#if false
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.CPlusPlusCompilerTool, typeof(VisualCCommon.CPlusPlusCompiler), typeof(VisualC.CPlusPlusCompilerOptionCollection))]
#endif
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.LinkerTool, typeof(VisualCCommon.Linker), typeof(VisualC.LinkerOptionCollection))]
#if false
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.Toolchain, typeof(VisualC.Toolchain), typeof(VisualC.ToolchainOptionCollection))]
#endif
[assembly: Opus.Core.MapToolChainClassTypes("C", "visualc", C.ClassNames.Win32ResourceCompilerTool, typeof(VisualCCommon.Win32ResourceCompiler), typeof(C.Win32ResourceCompilerOptionCollection))]

[assembly: C.RegisterToolchain(
    "visualc",
    typeof(VisualC.Toolset),
    typeof(VisualCCommon.CCompiler), typeof(VisualC.CCompilerOptionCollection),
    typeof(VisualCCommon.CxxCompiler), typeof(VisualC.CPlusPlusCompilerOptionCollection),
    typeof(VisualCCommon.Linker), typeof(VisualC.LinkerOptionCollection),
    typeof(VisualCCommon.Archiver), typeof(VisualC.ArchiverOptionCollection),
    typeof(VisualCCommon.Win32ResourceCompiler), typeof(C.Win32ResourceCompilerOptionCollection))]

namespace VisualC
{
    public class Solution
    {
        private static System.Guid ProjectTypeGuid;
        private static System.Guid SolutionFolderTypeGuid;

        static Solution()
        {
            // TODO: this path is for VCExpress - what about the professional version?
            using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\VCExpress\9.0\Projects"))
            {
                if (null == key)
                {
                    throw new Opus.Core.Exception("VisualStudio C++ Express 2008 was not installed");
                }

                string[] subKeyNames = key.GetSubKeyNames();
                foreach (string subKeyName in subKeyNames)
                {
                    using (Microsoft.Win32.RegistryKey subKey = key.OpenSubKey(subKeyName))
                    {
                        string projectExtension = subKey.GetValue("DefaultProjectExtension") as string;
                        if (null != projectExtension)
                        {
                            if (projectExtension == "vcproj")
                            {
                                ProjectTypeGuid = new System.Guid(subKeyName);
                                break;
                            }
                        }
                        string defaultValue = subKey.GetValue("") as string;
                        if (null != defaultValue)
                        {
                            if ("Solution Folder Project" == defaultValue)
                            {
                                SolutionFolderTypeGuid = new System.Guid(subKeyName);
                            }
                        }
                    }
                }
            }

            if (0 == ProjectTypeGuid.CompareTo(System.Guid.Empty))
            {
                throw new Opus.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2008");
            }

#if false
            // Note: do this instead of (null == Guid) to satify the Mono compiler
            // see CS0472, and something about struct comparisons
            if ((System.Nullable<System.Guid>)null == (System.Nullable<System.Guid>)ProjectTypeGuid)
            {
                throw new Opus.Core.Exception("Unable to locate VisualC project GUID for VisualStudio 2008");
            }
#endif
        }

        public string Header
        {
            get
            {
                System.Text.StringBuilder header = new System.Text.StringBuilder();
                header.AppendLine("Microsoft Visual Studio Solution File, Format Version 10.00");
                header.AppendLine("# Visual C++ Express 2008");
                return header.ToString();
            }
        }

        public System.Guid ProjectGuid
        {
            get
            {
                return ProjectTypeGuid;
            }
        }

        public System.Guid SolutionFolderGuid
        {
            get
            {
                return SolutionFolderTypeGuid;
            }
        }

        public string ProjectVersion
        {
            get
            {
                return "9.00";
            }
        }

        public string ProjectExtension
        {
            get
            {
                return ".vcproj";
            }
        }
    }
}
