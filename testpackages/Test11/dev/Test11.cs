// Automatically generated by Opus v0.00
namespace Test11
{
    // Define module classes here
    class CrossPlatformApplication : C.Application
    {
        public CrossPlatformApplication()
        {
            var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
            this.commonSourceFile.Include(sourceDir, "main.c");
            this.winSourceFile.Include(sourceDir, "win", "win.c");
            this.unixSourceFile.Include(sourceDir, "unix", "unix.c");
            this.osxSourceFile.Include(sourceDir, "osx", "osx.c");
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile commonSourceFile = new C.ObjectFile();

        [Opus.Core.SourceFiles(Platform=Opus.Core.EPlatform.Windows)]
        C.ObjectFile winSourceFile = new C.ObjectFile();

        [Opus.Core.SourceFiles(Platform=Opus.Core.EPlatform.Unix)]
        C.ObjectFile unixSourceFile = new C.ObjectFile();
        
        [Opus.Core.SourceFiles(Platform=Opus.Core.EPlatform.OSX)]
        C.ObjectFile osxSourceFile = new C.ObjectFile();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray WinVCDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.StringArray WinVCLibraries = new Opus.Core.StringArray("KERNEL32.lib");
    }
}
