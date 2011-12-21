// <copyright file="VSSolutionBuilder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.DeclareBuilder("VSSolution", typeof(VSSolutionBuilder.VSSolutionBuilder))]

namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder : Opus.Core.IBuilder
    {
        private static System.Type GetProjectClassType()
        {
            Opus.Core.PackageInformation toolchainPackage = Opus.Core.State.PackageInfo["VisualC"];
            if (null != toolchainPackage)
            {
                string projectClassTypeName = null;
                switch (toolchainPackage.Version)
                {
                    case "8.0":
                    case "9.0":
                        projectClassTypeName = "VSSolutionBuilder.VCProject";
                        break;

                    case "10.0":
                        projectClassTypeName = "VSSolutionBuilder.VCXBuildProject";
                        break;
                }

                System.Type projectClassType = System.Type.GetType(projectClassTypeName);
                return projectClassType;
            }
            else
            {
                toolchainPackage = Opus.Core.State.PackageInfo["DotNetFramework"];
                if (null != toolchainPackage)
                {
                    string projectClassTypeName = "VSSolutionBuilder.CSBuildProject";
                    System.Type projectClassType = System.Type.GetType(projectClassTypeName);
                    return projectClassType;
                }
                else
                {
                    throw new Opus.Core.Exception("Unable to locate a suitable toolchain package");
                }
            }
        }

        private static string CapitalizeFirstLetter(string word)
        {
            if (System.String.IsNullOrEmpty(word))
            {
                return System.String.Empty;
            }
            char[] a = word.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        private static string GetConfigurationNameFromTarget(Opus.Core.Target target)
        {
            string platform = GetPlatformNameFromTarget(target);
            string configurationName = System.String.Format("{0}|{1}", CapitalizeFirstLetter(target.Configuration.ToString()), platform);
            return configurationName;
        }

        private static string GetConfigurationNameFromTarget(Opus.Core.Target target, string platformName)
        {
            string configurationName = System.String.Format("{0}|{1}", CapitalizeFirstLetter(target.Configuration.ToString()), platformName);
            return configurationName;
        }

        public static string GetPlatformNameFromTarget(Opus.Core.Target target)
        {
            string platform;
            if (target.Platform == Opus.Core.EPlatform.Win32)
            {
                platform = "Win32";
            }
            else if (target.Platform == Opus.Core.EPlatform.Win64)
            {
                platform = "x64";
            }
            else
            {
                throw new Opus.Core.Exception("Only Win32 and Win64 are supported platforms for VisualStudio projects", false);
            }

            return platform;
        }

        internal static string RefactorPathForVCProj(string path, string outputDirectoryPath, string intermediateDirectoryPath, string projectName, System.Uri projectUri)
        {
            if (System.String.IsNullOrEmpty(path))
            {
                throw new Opus.Core.Exception("Cannot refactor an empty path for VisualStudio projects", false);
            }

            string refactoredPath = path;

            if (outputDirectoryPath != null)
            {
                string outputDir = outputDirectoryPath;
                if (!outputDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                {
                    outputDir += System.IO.Path.DirectorySeparatorChar;
                }
                refactoredPath = refactoredPath.Replace(outputDir, "$(OutDir)");
            }

            if (intermediateDirectoryPath != null)
            {
                string intDir = intermediateDirectoryPath;
                if (!intDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                {
                    intDir += System.IO.Path.DirectorySeparatorChar;
                }
                refactoredPath = refactoredPath.Replace(intDir, "$(IntDir)");
            }

            refactoredPath = refactoredPath.Replace(projectName, "$(ProjectName)");

            char splitter = refactoredPath[refactoredPath.Length - 1];
            if (!System.Char.IsLetterOrDigit(splitter))
            {
                string[] splitPath = refactoredPath.Split(new char[] { splitter });
                for (int i = 0; i < splitPath.Length; ++i)
                {
                    string split = splitPath[i];
                    if (System.String.IsNullOrEmpty(split))
                    {
                        continue;
                    }

                    split = split.Trim(new char[] { '\"' });

                    if (System.IO.Directory.Exists(split) || System.IO.File.Exists(split))
                    {
                        split = Opus.Core.RelativePathUtilities.GetPath(split, projectUri);
                        if (split.Contains(" "))
                        {
                            splitPath[i] = System.String.Format("\"{0}\"", split);
                        }
                        else
                        {
                            splitPath[i] = split;
                        }
                    }
                }
                refactoredPath = System.String.Join(splitter.ToString(), splitPath);
            }

            return refactoredPath;
        }

        private SolutionFile solutionFile;
    }
}
