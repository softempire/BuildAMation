// Automatically generated by Opus v0.00
namespace Test13
{
    // Define module classes here
    class QtApplication : C.Application
    {
        private const string WinTarget = "win.*-.*-.*";
        private const string WinVCTarget = "win.*-.*-visualc";
        private const string WinMingwTarget = "win.*-.*-mingw";

        class SourceFiles : C.CPlusPlus.ObjectFileCollection
        {
            public SourceFiles()
            {
                this.Add("source", "main.cpp");
            }

            class MocFiles : Qt.MocModule
            {
            }

            [Opus.Core.DependentModules]
            Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(typeof(SourceFiles.MocFiles));
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(
            typeof(Qt.Qt)
        );

        [Opus.Core.DependentModules(WinVCTarget)]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(
            typeof(WindowsSDK.WindowsSDK)
        );

        [C.RequiredLibraries(WinMingwTarget)]
        Opus.Core.StringArray winLibraries = new Opus.Core.StringArray(
            "-lQtCore4",
            "-lQtGui4"
        );

        [C.RequiredLibraries(WinVCTarget)]
        Opus.Core.StringArray winVCLibraries = new Opus.Core.StringArray(
            "KERNEL32.lib",
            "libQtCore4.a",
            "libQtGui4.a"
        );
    }
}
 