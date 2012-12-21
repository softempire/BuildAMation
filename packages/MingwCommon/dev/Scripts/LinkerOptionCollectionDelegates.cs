// Automatically generated file from OpusOptionInterfacePropertyGenerator.
// Command line:
// -i=..\..\..\C\dev\Scripts\ILinkerOptions.cs;ILinkerOptions.cs -n=MingwCommon -c=LinkerOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs -pv=PrivateData

namespace MingwCommon
{
    public partial class LinkerOptionCollection
    {
        #region C.ILinkerOptions Option delegates
        private static void OutputTypeCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ELinkerOutput> enumOption = option as Opus.Core.ValueTypeOption<C.ELinkerOutput>;
            LinkerOptionCollection options = sender as LinkerOptionCollection;
            switch (enumOption.Value)
            {
                case C.ELinkerOutput.Executable:
                case C.ELinkerOutput.DynamicLibrary:
                    string outputPathName = options.OutputFilePath;
                    if (outputPathName.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-o \"{0}\"", outputPathName));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-o {0}", outputPathName));
                    }
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized value for C.ELinkerOutput");
            }
        }
        private static void DoNotAutoIncludeStandardLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> ignoreStandardLibrariesOption = option as Opus.Core.ValueTypeOption<bool>;
            if (ignoreStandardLibrariesOption.Value)
            {
                commandLineBuilder.Add("-nostdlib");
            }
        }
        private static void DebugSymbolsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> debugSymbolsOption = option as Opus.Core.ValueTypeOption<bool>;
            if (debugSymbolsOption.Value)
            {
                commandLineBuilder.Add("-g");
            }
        }
        private static void SubSystemCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<C.ESubsystem> subSystemOption = option as Opus.Core.ValueTypeOption<C.ESubsystem>;
            switch (subSystemOption.Value)
            {
                case C.ESubsystem.NotSet:
                    // do nothing
                    break;
                case C.ESubsystem.Console:
                    commandLineBuilder.Add("-Wl,--subsystem,console");
                    break;
                case C.ESubsystem.Windows:
                    commandLineBuilder.Add("-Wl,--subsystem,windows");
                    break;
                default:
                    throw new Opus.Core.Exception("Unrecognized subsystem option");
            }
        }
        private static void DynamicLibraryCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> dynamicLibraryOption = option as Opus.Core.ValueTypeOption<bool>;
            if (dynamicLibraryOption.Value)
            {
                commandLineBuilder.Add("-shared");
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                if (null != options.StaticImportLibraryFilePath)
                {
                    if (options.StaticImportLibraryFilePath.Contains(" "))
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,--out-implib,\"{0}\"", options.StaticImportLibraryFilePath));
                    }
                    else
                    {
                        commandLineBuilder.Add(System.String.Format("-Wl,--out-implib,{0}", options.StaticImportLibraryFilePath));
                    }
                }
            }
        }
        private static void LibraryPathsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection> libraryPathsOption = option as Opus.Core.ReferenceTypeOption<Opus.Core.DirectoryCollection>;
            foreach (string libraryPath in libraryPathsOption.Value)
            {
                if (libraryPath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-L\"{0}\"", libraryPath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-L{0}", libraryPath));
                }
            }
        }
        private static void StandardLibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void LibrariesCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            // empty
        }
        private static void GenerateMapFileCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                LinkerOptionCollection options = sender as LinkerOptionCollection;
                if (options.MapFilePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-Wl,-Map,\"{0}\"", options.MapFilePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-Wl,-Map,{0}", options.MapFilePath));
                }
            }
        }
        private static void AdditionalOptionsCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ReferenceTypeOption<string> stringOption = option as Opus.Core.ReferenceTypeOption<string>;
            string[] arguments = stringOption.Value.Split(' ');
            foreach (string argument in arguments)
            {
                commandLineBuilder.Add(argument);
            }
        }
        #endregion
        #region ILinkerOptions Option delegates
        private static void EnableAutoImportCommandLineProcessor(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> boolOption = option as Opus.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-Wl,--enable-auto-import");
            }
        }
        #endregion
        protected override void SetDelegates(Opus.Core.DependencyNode node)
        {
            this["OutputType"].PrivateData = new PrivateData(OutputTypeCommandLineProcessor);
            this["DoNotAutoIncludeStandardLibraries"].PrivateData = new PrivateData(DoNotAutoIncludeStandardLibrariesCommandLineProcessor);
            this["DebugSymbols"].PrivateData = new PrivateData(DebugSymbolsCommandLineProcessor);
            this["SubSystem"].PrivateData = new PrivateData(SubSystemCommandLineProcessor);
            this["DynamicLibrary"].PrivateData = new PrivateData(DynamicLibraryCommandLineProcessor);
            this["LibraryPaths"].PrivateData = new PrivateData(LibraryPathsCommandLineProcessor);
            this["StandardLibraries"].PrivateData = new PrivateData(StandardLibrariesCommandLineProcessor);
            this["Libraries"].PrivateData = new PrivateData(LibrariesCommandLineProcessor);
            this["GenerateMapFile"].PrivateData = new PrivateData(GenerateMapFileCommandLineProcessor);
            this["AdditionalOptions"].PrivateData = new PrivateData(AdditionalOptionsCommandLineProcessor);
            this["EnableAutoImport"].PrivateData = new PrivateData(EnableAutoImportCommandLineProcessor);
        }
    }
}
