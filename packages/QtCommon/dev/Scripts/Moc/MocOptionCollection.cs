// <copyright file="MocOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    public sealed partial class MocOptionCollection : Opus.Core.BaseOptionCollection, CommandLineProcessor.ICommandLineSupport, IMocOptions
    {
        public MocOptionCollection()
            : base()
        {
        }

        public MocOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            IMocOptions options = this as IMocOptions;
            options.MocOutputPath = null;
            options.IncludePaths = new Opus.Core.DirectoryCollection();
            options.Defines = new C.DefineCollection();
            options.DoNotGenerateIncludeStatement = false;
            options.DoNotDisplayWarnings = false;
            options.PathPrefix = null;

            // version number of the current Qt package
            string QtVersion = Opus.Core.State.PackageInfo["Qt"].Version;
            string QtVersionFormatted = QtVersion.Replace(".", "0");
            string VersionDefine = "QT_VERSION=0x0" + QtVersionFormatted;
            options.Defines.Add(VersionDefine);
        }

        public override void SetNodeOwnership(Opus.Core.DependencyNode node)
        {
            string mocDir = node.GetTargettedModuleBuildDirectory("src");
            MocFile mocFile = node.Module as MocFile;
            string mocPath;
            if (null != mocFile)
            {
                string sourceFilePath = mocFile.SourceFile.AbsolutePath;
                string filename = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
                mocPath = System.IO.Path.Combine(mocDir, System.String.Format("{0}{1}.cpp", MocFile.Prefix, filename));
            }
            else
            {
                // TODO: would like to have a null output path for a collection, but it doesn't work for cloning reference types
                string filename = node.ModuleName;
                mocPath = System.IO.Path.Combine(mocDir, System.String.Format("{0}{1}.cpp", MocFile.Prefix, filename));
            }

            (this as IMocOptions).MocOutputPath = mocPath;
        }

        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["MocOutputPath"].PrivateData = new MocPrivateData(MocOutputPathCommandLine);
            this["IncludePaths"].PrivateData = new MocPrivateData(IncludePathsCommandLine);
            this["Defines"].PrivateData = new MocPrivateData(DefinesCommandLine);
            this["DoNotGenerateIncludeStatement"].PrivateData = new MocPrivateData(DoNotGenerateIncludeStatementCommandLine);
            this["DoNotDisplayWarnings"].PrivateData = new MocPrivateData(DoNotDisplayWarningsCommandLine);
            this["PathPrefix"].PrivateData = new MocPrivateData(PathPrefixCommandLine);
        }

        public override void FinalizeOptions(Opus.Core.Target target)
        {
            if (null == this.OutputPaths[OutputFileFlags.MocGeneratedSourceFile])
            {
                this.OutputPaths[OutputFileFlags.MocGeneratedSourceFile] = (this as IMocOptions).MocOutputPath;
            }

            base.FinalizeOptions(target);
        }

        private static void MocOutputPathCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            if (stringOption.Value.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("-o\"{0}\"", stringOption.Value));
            }
            else
            {
                commandLineBuilder.Add(System.String.Format("-o{0}", stringOption.Value));
            }
        }

        private static void IncludePathsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> directoryCollectionOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string directory in directoryCollectionOption.Value)
            {
                if (directory.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-I\"{0}\"", directory));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-I{0}", directory));
                }
            }
        }

        private static void DefinesCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<C.DefineCollection> definesCollectionOption = option as Opus.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (string directory in definesCollectionOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-D{0}", directory));
            }
        }

        private static void DoNotGenerateIncludeStatementCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-i");
            }
        }

        private static void DoNotDisplayWarningsCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-nw");
            }
        }

        private static void PathPrefixCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            if (stringOption.Value != null)
            {
                commandLineBuilder.Add(System.String.Format("-p {0}", stringOption.Value));
            }
        }

        void CommandLineProcessor.ICommandLineSupport.ToCommandLineArguments(Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            CommandLineProcessor.ToCommandLine.Execute(this, commandLineBuilder, target);
        }

        Opus.Core.DirectoryCollection CommandLineProcessor.ICommandLineSupport.DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection dirsToCreate = new Opus.Core.DirectoryCollection();

            IMocOptions options = this as IMocOptions;
            if (null != options.MocOutputPath)
            {
                string mocDir = System.IO.Path.GetDirectoryName(options.MocOutputPath);
                dirsToCreate.AddAbsoluteDirectory(mocDir, false);
            }

            return dirsToCreate;
        }
    }
}