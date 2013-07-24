// Automatically generated by Opus v0.30
namespace OpenCLTest1
{
    // Add modules here
    class OpenCLTest1 : C.Application
    {
        class SourceFiles : C.Cxx.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
                this.Include(sourceDir, "*.cpp");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

#if USINGAMD
        [Opus.Core.DependentModules]
        Opus.Core.TypeArray dependents = new Opus.Core.TypeArray(typeof(AMDAPPSDK.AMDAPPSDK));
#endif

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray("KERNEL32.lib");
    }

#if OPUSPACKAGE_FILEUTILITIES_DEV
    class CopyKernels : FileUtilities.CopyFileCollection
    {
        public CopyKernels()
        {
            var dataDir = this.Locations["PackageDir"].SubDirectory("data");
            this.Include(dataDir, "*.cl");
        }

        [FileUtilities.BesideModule(C.OutputFileFlags.Executable)]
        System.Type nextTo = typeof(OpenCLTest1);
    }
#elif OPUSPACKAGE_FILEUTILITIES_1_0
    class CopyKernels : FileUtilities.CopyFiles
    {
        public CopyKernels(Opus.Core.Target target)
        {
            this.sourceFiles.Include(this, "data", "*.cl");
        }

        [Opus.Core.SourceFiles]
        Opus.Core.FileCollection sourceFiles = new Opus.Core.FileCollection();

        [FileUtilities.DestinationModuleDirectory(C.OutputFileFlags.Executable)]
        Opus.Core.TypeArray destinationTarget = new Opus.Core.TypeArray(typeof(OpenCLTest1));
    }
#else
#error Unknown FileUtilities package version
#endif
}
