// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportLinkerOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalLinkerOptionsDelegateAttribute : System.Attribute
    {
    }

    public abstract class Linker
    {
        public abstract string ExecutableCPlusPlus(Opus.Core.Target target);

        protected abstract string StartLibraryList
        {
            get;
        }

        protected abstract string EndLibraryList
        {
            get;
        }

        public void AppendLibrariesToCommandLine(Opus.Core.StringArray commandLineBuilder,
                                                 ILinkerOptions linkerOptions,
                                                 Opus.Core.StringArray otherLibraryPaths)
        {
            if ((linkerOptions.DoNotAutoIncludeStandardLibraries && linkerOptions.StandardLibraries.Count > 0) ||
                (linkerOptions.Libraries.Count > 0) ||
                ((null != otherLibraryPaths) && (otherLibraryPaths.Count > 0)))
            {
                commandLineBuilder.Add(this.StartLibraryList);
                if (linkerOptions.DoNotAutoIncludeStandardLibraries)
                {
                    foreach (string standardLibraryPath in linkerOptions.StandardLibraries)
                    {
                        commandLineBuilder.Add(System.String.Format("\"{0}\"", standardLibraryPath));
                    }
                }
                foreach (string libraryPath in linkerOptions.Libraries)
                {
                    commandLineBuilder.Add(System.String.Format("\"{0}\"", libraryPath));
                }
                if (otherLibraryPaths != null)
                {
                    commandLineBuilder.Add(otherLibraryPaths.ToString(' '));
                }
                commandLineBuilder.Add(this.EndLibraryList);
            }
        }
    }
}