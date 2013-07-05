// <copyright file="QMakeNodeData.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QMakeBuilder package</summary>
// <author>Mark Final</author>

namespace QMakeBuilder
{
    public class QMakeData
    {
        [System.Flags]
        public enum OutputType
        {
            None           = 0,
            Undefined      = (1 << 0),
            ObjectFile     = (1 << 1),
            WinResource    = (1 << 2),
            MocFile        = (1 << 3),
            StaticLibrary  = (1 << 4),
            DynamicLibrary = (1 << 5),
            Application    = (1 << 6)
        }

        public QMakeData(Opus.Core.DependencyNode node)
        {
            this.OwningNode = node;
            this.Defines = new Opus.Core.StringArray();
            this.DestDir = string.Empty;
            this.Headers = new Opus.Core.StringArray();
            this.IncludePaths = new Opus.Core.StringArray();
            this.Libraries = new Opus.Core.StringArray();
            this.Merged = false;
            this.MocDir = string.Empty;
            this.ObjectsDir = string.Empty;
            this.Output = OutputType.Undefined;
            this.PriPaths = new Opus.Core.StringArray();
            this.QtModules = new Opus.Core.StringArray();
            this.Sources = new Opus.Core.StringArray();
            this.WinRCFiles = new Opus.Core.StringArray();
        }

        public Opus.Core.DependencyNode OwningNode
        {
            get;
            private set;
        }

        public Opus.Core.StringArray Defines
        {
            get;
            private set;
        }

        public string DestDir
        {
            get;
            set;
        }

        public Opus.Core.StringArray Headers
        {
            get;
            private set;
        }

        public Opus.Core.StringArray IncludePaths
        {
            get;
            private set;
        }

        public Opus.Core.StringArray Libraries
        {
            get;
            private set;
        }

        private bool Merged
        {
            get;
            set;
        }

        public string MocDir
        {
            get;
            set;
        }

        public string ObjectsDir
        {
            get;
            set;
        }

        public string ProFilePath
        {
            get;
            private set;
        }

        public Opus.Core.StringArray PriPaths
        {
            get;
            private set;
        }

        public Opus.Core.StringArray QtModules
        {
            get;
            private set;
        }

        public Opus.Core.StringArray Sources
        {
            get;
            private set;
        }

        public OutputType Output
        {
            get;
            set;
        }

        public Opus.Core.StringArray WinRCFiles
        {
            get;
            private set;
        }

        // TODO: need to decide what to do with this
        class Values<T>
        {
            public T Debug
            {
                get;
                set;
            }

            public T Release
            {
                get;
                set;
            }
        }

        private static string FormatPath(string path, string proFilePath)
        {
            // make the path relative to the .pro
            var newPath = (null != proFilePath) ? Opus.Core.RelativePathUtilities.GetPath(path, proFilePath) : path;

            // QMake warning: unescaped backslashes are deprecated
            newPath = newPath.Replace("\\", "/");

            // spaces in paths need to be quoted
            if (newPath.Contains(" "))
            {
                newPath = System.String.Format("$$quote({0})", newPath);
            }

            return newPath;
        }

        private static string PathListToString(Opus.Core.StringArray pathList, string proFilePath)
        {
            var escapedPathList = new Opus.Core.StringArray();
            foreach (var path in pathList)
            {
                escapedPathList.Add(FormatPath(path, proFilePath));
            }

            return escapedPathList.ToString("\\\n\t");
        }

        private static void WriteTemplate(Opus.Core.Array<QMakeData> array,
                                          string proFilePath,
                                          System.IO.StreamWriter writer)
        {
            switch (array[0].Output)
            {
                case OutputType.Application:
                    writer.WriteLine("TEMPLATE = app");
                    break;

                case OutputType.StaticLibrary:
                case OutputType.DynamicLibrary:
                    writer.WriteLine("TEMPLATE = lib");
                    break;

                case OutputType.ObjectFile:
                    // special case - a static library which has the least side-effects
                    writer.WriteLine("TEMPLATE = lib");
                    break;

                default:
                    throw new Opus.Core.Exception("Should not be writing out .pro files for outputs of type '{0}'", array[0].Output.ToString());
            }
        }

        private static void WriteConfig(Opus.Core.Array<QMakeData> array,
                                        string proFilePath,
                                        System.IO.StreamWriter writer)
        {
            string config = string.Empty;
            if (array.Count == 1)
            {
                if (array[0].OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                {
                    config += " debug";
                }
                else
                {
                    config += " release";
                }
            }
            else
            {
                config += " debug_and_release";
            }

            var qtModules = array[0].QtModules;
            if (array.Count > 1)
            {
                qtModules = new Opus.Core.StringArray(qtModules.Union(array[1].QtModules));
            }
            if (qtModules.Count > 0)
            {
                config += " qt";
            }

            // special case of an object file creating a static lib too
            if (0 != (array[0].Output & (OutputType.StaticLibrary | OutputType.ObjectFile)))
            {
                config += " staticlib";
            }
            else if (array[0].Output == OutputType.DynamicLibrary)
            {
                config += " shared";
            }
            writer.WriteLine("CONFIG +={0}", config);
            if (qtModules.Count > 0)
            {
                writer.WriteLine("QT = {0}", qtModules.ToString());
            }
        }

        private static void WriteSources(Opus.Core.Array<QMakeData> array,
                                         string proFilePath,
                                         System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].Sources, "SOURCES+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.Sources;
                    }
                    else
                    {
                        values.Release = data.Sources;
                    }
                }

                WriteStringArrays(values, "SOURCES+=", proFilePath, writer);
            }
        }

        private static void WriteHeaders(Opus.Core.Array<QMakeData> array,
                                         string proFilePath,
                                         System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].Headers, "HEADERS+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.Headers;
                    }
                    else
                    {
                        values.Release = data.Headers;
                    }
                }

                WriteStringArrays(values, "HEADERS+=", proFilePath, writer);
            }
        }

        private static void WriteDestDir(Opus.Core.Array<QMakeData> array,
                                         string proFilePath,
                                         System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteString(array[0].DestDir, "DESTDIR=", null, writer);
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.DestDir;
                    }
                    else
                    {
                        values.Release = data.DestDir;
                    }
                }

                WriteString(values, "DESTDIR=", null, writer);
            }
        }

        private static void WriteMocDir(Opus.Core.Array<QMakeData> array,
                                        string proFilePath,
                                        System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteString(array[0].MocDir, "MOC_DIR=", null, writer);
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.MocDir;
                    }
                    else
                    {
                        values.Release = data.MocDir;
                    }
                }

                WriteString(values, "MOC_DIR=", null, writer);
            }
        }

        private static void WriteObjectsDir(Opus.Core.Array<QMakeData> array,
                                            string proFilePath,
                                            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteString(array[0].ObjectsDir, "OBJECTS_DIR=", null, writer);
            }
            else
            {
                var values = new Values<string>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.ObjectsDir;
                    }
                    else
                    {
                        values.Release = data.ObjectsDir;
                    }
                }

                WriteString(values, "OBJECTS_DIR=", null, writer);
            }
        }

        private static void WriteIncludePaths(Opus.Core.Array<QMakeData> array, string proFilePath, System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].IncludePaths, "INCLUDEPATH+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.IncludePaths;
                    }
                    else
                    {
                        values.Release = data.IncludePaths;
                    }
                }

                WriteStringArrays(values, "INCLUDEPATH+=", proFilePath, writer);
            }
        }

        private static void WriteDefines(Opus.Core.Array<QMakeData> array, string proFilePath, System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].Defines, "DEFINES+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.Defines;
                    }
                    else
                    {
                        values.Release = data.Defines;
                    }
                }

                WriteStringArrays(values, "DEFINES+=", proFilePath, writer);
            }
        }

        private static void WriteString(string value, string format, string proFilePath, System.IO.StreamWriter writer)
        {
            if (0 == value.Length)
            {
                return;
            }
            if (!format.Contains("{0}"))
            {
                format += "{0}";
            }
            writer.WriteLine(format, FormatPath(value, proFilePath));
        }

        private static void WriteStringArray(Opus.Core.StringArray stringArray,
                                             string format,
                                             string proFilePath,
                                             System.IO.StreamWriter writer)
        {
            if (0 == stringArray.Count)
            {
                return;
            }
            else if (1 == stringArray.Count)
            {
                WriteString(stringArray[0], format, proFilePath, writer);
            }
            else
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Append(format);
                foreach (string value in stringArray)
                {
                    builder.AppendFormat("\\\n\t{0}", FormatPath(value, proFilePath));
                }
                writer.WriteLine(builder.ToString());
            }
        }

        private static void WriteString(Values<string> values, string format, string proFilePath, System.IO.StreamWriter writer)
        {
            if (values.Debug == values.Release)
            {
                WriteString(values.Debug, format, proFilePath, writer);
            }
            else
            {
                // see the following for an explanation of this syntax
                // http://qt-project.org/faq/answer/what_does_the_syntax_configdebugdebugrelease_mean_what_does_the_1st_argumen
                WriteString(values.Debug, "CONFIG(debug,debug|release):" + format, proFilePath, writer);
                WriteString(values.Release, "CONFIG(release,debug|release):" + format, proFilePath, writer);
            }
        }

        private static void WriteStringArrays(Values<Opus.Core.StringArray> values, string format, string proFilePath, System.IO.StreamWriter writer)
        {
            var intersect = new Opus.Core.StringArray(values.Debug.Intersect(values.Release));
            WriteStringArray(intersect, format, proFilePath, writer);

            // see the following for an explanation of this syntax
            // http://qt-project.org/faq/answer/what_does_the_syntax_configdebugdebugrelease_mean_what_does_the_1st_argumen
            if (intersect.Count != values.Debug.Count)
            {
                var debugOnly = new Opus.Core.StringArray(values.Debug.Complement(intersect));
                WriteStringArray(debugOnly, "CONFIG(debug,debug|release):" + format, proFilePath, writer);
            }
            if (intersect.Count != values.Release.Count)
            {
                var releaseOnly = new Opus.Core.StringArray(values.Release.Complement(intersect));
                WriteStringArray(releaseOnly, "CONFIG(release,debug|release):" + format, proFilePath, writer);
            }
        }

        private static void WritePriPaths(Opus.Core.Array<QMakeData> array,
                                          string proFilePath,
                                          System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].PriPaths, @"include({0})", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.PriPaths;
                    }
                    else
                    {
                        values.Release = data.PriPaths;
                    }
                }

                WriteStringArrays(values, @"include({0})", proFilePath, writer);
            }
        }

        private static void WriteWinRCFiles(Opus.Core.Array<QMakeData> array,
                                            string proFilePath,
                                            System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].WinRCFiles, "RC_FILE+=", proFilePath, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.WinRCFiles;
                    }
                    else
                    {
                        values.Release = data.WinRCFiles;
                    }
                }

                WriteStringArrays(values, "RC_FILE+=", proFilePath, writer);
            }
        }

        private static void WriteLibraries(Opus.Core.Array<QMakeData> array,
                                           string proFilePath,
                                           System.IO.StreamWriter writer)
        {
            if (1 == array.Count)
            {
                WriteStringArray(array[0].Libraries, "LIBS=", null, writer);
            }
            else
            {
                var values = new Values<Opus.Core.StringArray>();
                foreach (var data in array)
                {
                    if (data.OwningNode.Target.HasConfiguration(Opus.Core.EConfiguration.Debug))
                    {
                        values.Debug = data.Libraries;
                    }
                    else
                    {
                        values.Release = data.Libraries;
                    }
                }

                WriteStringArrays(values, "LIBS=", null, writer);
            }
        }

        public void Merge(QMakeData data)
        {
            this.Merge(data, OutputType.None);
        }

        public void Merge(QMakeData data, OutputType excludeType)
        {
            if (0 != (data.Output & excludeType))
            {
                return;
            }

            var baseTargetLHS = (Opus.Core.BaseTarget)(this.OwningNode.Target);
            var baseTargetRHS = (Opus.Core.BaseTarget)(data.OwningNode.Target);
            if (baseTargetLHS != baseTargetRHS)
            {
                throw new Opus.Core.Exception("Cannot merge data from different Opus.Core.BaseTargets: {0} vs {1}", baseTargetLHS.ToString(), baseTargetRHS.ToString());
            }

            this.Defines.AddRangeUnique(data.Defines);
            if (data.DestDir.Length > 0)
            {
                this.DestDir = data.DestDir;
            }
            this.Headers.AddRangeUnique(data.Headers);
            this.IncludePaths.AddRangeUnique(data.IncludePaths);
            this.Libraries.AddRangeUnique(data.Libraries);
            if (data.MocDir.Length > 0)
            {
                this.MocDir = data.MocDir;
            }
            if (data.ObjectsDir.Length > 0)
            {
                this.ObjectsDir = data.ObjectsDir;
            }
            this.PriPaths.AddRangeUnique(data.PriPaths);
            this.QtModules.AddRangeUnique(data.QtModules);
            this.Sources.AddRangeUnique(data.Sources);
            if (data.Output != OutputType.Undefined)
            {
                this.Output = data.Output;
            }
            this.WinRCFiles.AddRangeUnique(data.WinRCFiles);

            data.Merged = true;
        }

        public static void Write(Opus.Core.Array<QMakeData> array)
        {
            bool consistentMergeState = true;
            if (array.Count > 1)
            {
                foreach (var data in array)
                {
                    if (data.Merged != array[0].Merged)
                    {
                        consistentMergeState = false;
                    }
                }
            }
            if (!consistentMergeState)
            {
                throw new Opus.Core.Exception("Data is inconsistently merged");
            }
            if (array[0].Merged)
            {
                Opus.Core.Log.MessageAll("Not writing a pro file as qmake data is merged");
                return;
            }

            var node = array[0].OwningNode;
            string proFileDirectory = node.GetModuleBuildDirectory();
            string proFilePath = System.IO.Path.Combine(proFileDirectory, System.String.Format("{0}.pro", node.ModuleName));
            Opus.Core.Log.MessageAll("QMake Pro File for node '{0}': '{1}'", node.UniqueModuleName, proFilePath);
            foreach (var data in array)
            {
                data.ProFilePath = proFilePath;
            }

            // TODO: this might be temporary
            System.IO.Directory.CreateDirectory(proFileDirectory);

            using (var proWriter = new System.IO.StreamWriter(proFilePath))
            {
                WritePriPaths(array, proFilePath, proWriter);
                WriteTemplate(array, proFilePath, proWriter);
                WriteConfig(array, proFilePath, proWriter);
                WriteDestDir(array, proFilePath, proWriter);
                WriteMocDir(array, proFilePath, proWriter);
                WriteObjectsDir(array, proFilePath, proWriter);
                WriteIncludePaths(array, proFilePath, proWriter);
                WriteDefines(array, proFilePath, proWriter);
                WriteSources(array, proFilePath, proWriter);
                WriteHeaders(array, proFilePath, proWriter);
                WriteWinRCFiles(array, proFilePath, proWriter);
                WriteLibraries(array, proFilePath, proWriter);
            }
        }
    }

    // old
#if true
    public class NodeData
    {
        private System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> Dictionary = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();
        private System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> SingleValueDictionary = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();

        public NodeData()
        {
            this.HasPostLinks = false;
        }

        public string Configuration
        {
            get;
            set;
        }

        public string ProFilePathName
        {
            get;
            set;
        }

        public bool HasPostLinks
        {
            get;
            set;
        }

        public bool Contains(string VariableName)
        {
            bool contains = this.Dictionary.ContainsKey(VariableName) || this.SingleValueDictionary.ContainsKey(VariableName);
            return contains;
        }

        public Opus.Core.StringArray this[string VariableName]
        {
            get
            {
                if (!this.Dictionary.ContainsKey(VariableName) && !this.SingleValueDictionary.ContainsKey(VariableName))
                {
                    throw new Opus.Core.Exception("Unable to locate variable '{0}'", VariableName);
                }

                if (this.Dictionary.ContainsKey(VariableName))
                {
                    return this.Dictionary[VariableName];
                }

                return this.SingleValueDictionary[VariableName];
            }
        }

        public void AddVariable(string VariableName, string Value)
        {
            Dictionary.Add(VariableName, new Opus.Core.StringArray(Value));
        }

        public void AddUniqueVariable(string VariableName, Opus.Core.StringArray Value)
        {
            try
            {
                SingleValueDictionary.Add(VariableName, Value);
            }
            catch (System.ArgumentException)
            {
                Opus.Core.Log.MessageAll("INVESTIGATE: Variable '{0}' already exists in dictionary; existing value '{1}', incoming value = '{2}'", VariableName, SingleValueDictionary[VariableName], Value);
            }
        }

        public void Merge(NodeData data)
        {
            if (null == data)
            {
                return;
            }

            if (this.Configuration != data.Configuration)
            {
                throw new Opus.Core.Exception("Cannot merge data from different configurations");
            }

            foreach (System.Collections.Generic.KeyValuePair<string, Opus.Core.StringArray> entry in data.Dictionary)
            {
                if (this.Dictionary.ContainsKey(entry.Key))
                {
                    this.Dictionary[entry.Key].AddRange(entry.Value);
                }
                else
                {
                    this.Dictionary.Add(entry.Key, entry.Value);
                }
            }

            foreach (System.Collections.Generic.KeyValuePair<string, Opus.Core.StringArray> entry in data.SingleValueDictionary)
            {
                if (this.SingleValueDictionary.ContainsKey(entry.Key))
                {
                    //throw new Opus.Core.Exception("Repeated entry for '{0}':\nwas '{1}'\nwanted '{2}'", entry.Key, this.SingleValueDictionary[entry.Key], entry.Value);

                    // TODO: this isn't particularly great, as there may be conflicting arguments
                    foreach (string item in entry.Value)
                    {
                        if (!this.SingleValueDictionary[entry.Key].Contains(item))
                        {
                            this.SingleValueDictionary[entry.Key].Add(item);
                        }
                    }
                }
                else
                {
                    this.SingleValueDictionary.Add(entry.Key, entry.Value);
                }
            }
        }
    }
#endif // old
}
