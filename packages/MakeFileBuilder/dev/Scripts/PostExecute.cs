// <copyright file="PostExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder :
        Opus.Core.IBuilderPostExecute
    {
        private class UniquePathCollection
        {
            private Opus.Core.StringArray pathList = new Opus.Core.StringArray();

            public void
            Add(
                string path)
            {
                if (!System.String.IsNullOrEmpty(path) && !pathList.Contains(path))
                {
                    pathList.Add(path);
                }
            }

            public System.Collections.Generic.IEnumerator<string>
            GetEnumerator()
            {
                return this.pathList.GetEnumerator();
            }

            public override string
            ToString()
            {
                // TODO: check whether the separator needs to be different on Linux and OSX?

                var builder = new System.Text.StringBuilder();
                foreach (var environmentPath in this.pathList)
                {
                    builder.AppendFormat("{0};", environmentPath);
                }
                return builder.ToString();
            }
        }

        #region IBuilderPostExecute Members

        void
        Opus.Core.IBuilderPostExecute.PostExecute(
            Opus.Core.DependencyNodeCollection executedNodes)
        {
            Opus.Core.Log.DebugMessage("PostExecute for MakeFiles");

            if (0 == executedNodes.Count)
            {
                Opus.Core.Log.Info("No MakeFile written as there were no targets generated");
                return;
            }

            string targetList = null;
            var environmentPaths = new UniquePathCollection(); // TODO: redundant
            var environment = new System.Collections.Generic.Dictionary<string, UniquePathCollection>();
            foreach (var node in executedNodes)
            {
                var data = node.Data as MakeFileData;
                if (data != null)
                {
                    // if a node has no parent, then it is a good choice for exposing the
                    // target for in the main Makefile. However, some false-positives can
                    // arise from this for orphaned modules that are dependents from child
                    // modules (e.g. code generators)
                    if (null == node.Parent)
                    {
                        foreach (var targetNames in data.TargetDictionary.Values)
                        {
                            foreach (var targetName in targetNames)
                            {
                                targetList += targetName + " ";
                            }
                        }
                    }

                    if (data.Environment != null)
                    {
                        foreach (var key in data.Environment.Keys)
                        {
                            if (!environment.ContainsKey(key))
                            {
                                environment[key] = new UniquePathCollection();
                            }

                            foreach (var path in data.Environment[key])
                            {
                                environment[key].Add(path);
                            }
                        }
                    }
                }
            }

            // write top level Makefile
            {
                Opus.Core.Log.DebugMessage("Makefile : '{0}'", this.topLevelMakeFilePath);
                using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(this.topLevelMakeFilePath))
                {
                    makeFileWriter.WriteLine("# Record the current directory");
                    if (Opus.Core.OSUtilities.IsWindowsHosting)
                    {
                        makeFileWriter.WriteLine("CURDIR := $(subst /,\\,$(realpath .))");
                    }
                    else
                    {
                        makeFileWriter.WriteLine("CURDIR := $(realpath .)");
                    }
                    makeFileWriter.WriteLine("");

                    makeFileWriter.WriteLine("# Default goal");
                    makeFileWriter.WriteLine(".PHONY: all");
                    makeFileWriter.WriteLine("all: {0}", targetList);
                    makeFileWriter.WriteLine("");

                    makeFileWriter.WriteLine("# Remove all implicit rule suffixes");
                    makeFileWriter.WriteLine(".SUFFIXES:");
                    makeFileWriter.WriteLine("");

                    if (null != environmentPaths)
                    {
                        makeFileWriter.WriteLine("# Environment PATH for all tools");
                        makeFileWriter.WriteLine("INITIALPATH := $(PATH)");
                        makeFileWriter.WriteLine("export PATH := {0}$(INITIALPATH)", environmentPaths.ToString());
                        makeFileWriter.WriteLine("");
                    }

                    if (null != environment && environment.Count > 0)
                    {
                        makeFileWriter.WriteLine("# Environment variables for all tools");
                        foreach (var key in environment.Keys)
                        {
                            makeFileWriter.WriteLine("INITIAL{0} := $({0})", key);
                            makeFileWriter.WriteLine("export {0} := {1}$(INITIAL{0})", key, environment[key].ToString());
                        }
                        makeFileWriter.WriteLine("");
                    }

                    makeFileWriter.WriteLine("# include all sub-makefiles");
                    foreach (var node in executedNodes)
                    {
                        var data = node.Data as MakeFileData;
                        if (data != null)
                        {
                            var relativeDataFile = Opus.Core.RelativePathUtilities.GetPath(data.MakeFilePath, this.topLevelMakeFilePath, "$(CURDIR)");
                            makeFileWriter.WriteLine("include {0}", relativeDataFile);
                        }
                    }
                    makeFileWriter.WriteLine("");

                    makeFileWriter.WriteLine("# Create any directories necessary");
                    makeFileWriter.WriteLine("$(sort $(builddirs)):");
                    if (Opus.Core.OSUtilities.IsWindowsHosting)
                    {
                        makeFileWriter.WriteLine("\t-mkdir $@");
                    }
                    else
                    {
                        makeFileWriter.WriteLine("\t-mkdir -p $@");
                    }
                    makeFileWriter.WriteLine("");

                    makeFileWriter.WriteLine("# Delete generated files");
                    makeFileWriter.WriteLine(".PHONY: clean");
                    makeFileWriter.WriteLine("clean:");
                    if (Opus.Core.OSUtilities.IsWindowsHosting)
                    {
                        makeFileWriter.WriteLine("\t-rmdir /S /Q $(sort $(builddirs)) 2>nul");
                    }
                    else
                    {
                        makeFileWriter.WriteLine("\t-rm -f -r $(sort $(builddirs)) >nul");
                    }
                    makeFileWriter.WriteLine("");
                }
            }

            Opus.Core.Log.Info("Successfully created MakeFile for package '{0}'\n\t{1}", Opus.Core.State.PackageInfo[0].Name, this.topLevelMakeFilePath);
        }

        #endregion
    }
}
