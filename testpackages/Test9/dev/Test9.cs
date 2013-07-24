// Automatically generated by Opus v0.00
namespace Test9
{
    // Define module classes here
    class CFile : C.ObjectFile
    {
        public CFile()
        {
            var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
            this.Include(sourceDir, "main_c.c");
        }
    }

    class CFileCollection : C.ObjectFileCollection
    {
        public CFileCollection()
        {
            var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
            this.Include(sourceDir, "main_c.c");
        }
    }

    class CppFile : C.Cxx.ObjectFile
    {
        public CppFile()
        {
            var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
            this.Include(sourceDir, "main_cpp.c");
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(CppFile_UpdateOptions);
        }

        void CppFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICxxCompilerOptions;
            compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
        }
    }

    class CppFileCollection : C.Cxx.ObjectFileCollection
    {
        public CppFileCollection()
        {
            var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
            this.Include(sourceDir, "main_cpp.c");

            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(CppFileCollection_UpdateOptions);
        }

        void CppFileCollection_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICxxCompilerOptions;
            compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
        }
    }

    class MixedLanguageApplication : C.Application
    {
        public MixedLanguageApplication()
        {
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SetSystemLibraries);
        }

        static void SetSystemLibraries(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var linkerOptions = module.Options as C.ILinkerOptions;
            if (Opus.Core.OSUtilities.IsWindows(target))
            {
                if (linkerOptions is VisualC.LinkerOptionCollection)
                {
                    linkerOptions.Libraries.Add("KERNEL32.lib");
                }
            }
        }

        class CSourceFiles : C.ObjectFileCollection
        {
            public CSourceFiles()
            {
                var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
                this.Include(sourceDir, "library_c.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(IncludePaths);
            }

            void IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "include");
            }
        }

        class CxxSourceFiles : C.Cxx.ObjectFileCollection
        {
            public CxxSourceFiles()
            {
                var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
                this.Include(sourceDir, "library_cpp.c");
                this.Include(sourceDir, "appmain_cpp.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(CxxSourceFiles_UpdateOptions);
                this.UpdateOptions += IncludePaths;
            }

            void IncludePaths(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "include");
            }

            void CxxSourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICxxCompilerOptions;
                compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            }
        }

        [Opus.Core.SourceFiles]
        CSourceFiles cSourceFiles = new CSourceFiles();
        [Opus.Core.SourceFiles]
        CxxSourceFiles cppSourceFiles = new CxxSourceFiles();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));
    }

    class CStaticLibraryFromFile : C.StaticLibrary
    {
        public CStaticLibraryFromFile()
        {
            var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
            this.sourceFile.Include(sourceDir, "library_c.c");
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
        }

        void sourceFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "include");
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();
    }

    class CStaticLibraryFromCollection : C.StaticLibrary
    {
        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
                this.Include(sourceDir, "library_c.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "include");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();
    }

    class CppStaticLibraryFromFile : C.StaticLibrary
    {
        public CppStaticLibraryFromFile()
        {
            var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
            this.sourceFile.Include(sourceDir, "library_cpp.c");
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_ExceptionHandling);
        }

        void sourceFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "include");
        }

        void sourceFile_ExceptionHandling(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICxxCompilerOptions;
            compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
        }

        [Opus.Core.SourceFiles]
        C.Cxx.ObjectFile sourceFile = new C.Cxx.ObjectFile();
    }

    class CppStaticLibaryFromCollection : C.StaticLibrary
    {
        class SourceFiles : C.Cxx.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
                this.Include(sourceDir, "library_cpp.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_ExceptionHandling);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "include");
            }

            void SourceFiles_ExceptionHandling(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICxxCompilerOptions;
                compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();
    }

    class CDynamicLibraryFromFile : C.DynamicLibrary
    {
        public CDynamicLibraryFromFile()
        {
            var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
            this.sourceFile.Include(sourceDir, "library_c.c");
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
        }

        void sourceFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "include");
        }

        [Opus.Core.SourceFiles]
        C.ObjectFile sourceFile = new C.ObjectFile();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray("KERNEL32.lib");
    }

    class CDynamicLibraryFromCollection : C.DynamicLibrary
    {
        class SourceFiles : C.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
                this.Include(sourceDir, "library_c.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "include");
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray("KERNEL32.lib");
    }

    class CppDynamicLibraryFromFile : C.DynamicLibrary
    {
        public CppDynamicLibraryFromFile()
        {
            var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
            this.sourceFile.Include(sourceDir, "library_cpp.c");
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_UpdateOptions);
            this.sourceFile.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(sourceFile_ExceptionHandling);
        }

        void sourceFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICCompilerOptions;
            compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "include");
        }

        void sourceFile_ExceptionHandling(Opus.Core.IModule module, Opus.Core.Target target)
        {
            var compilerOptions = module.Options as C.ICxxCompilerOptions;
            compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
        }

        [Opus.Core.SourceFiles]
        C.Cxx.ObjectFile sourceFile = new C.Cxx.ObjectFile();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray("KERNEL32.lib");
    }

    class CppDynamicLibaryFromCollection : C.DynamicLibrary
    {
        class SourceFiles : C.Cxx.ObjectFileCollection
        {
            public SourceFiles()
            {
                var sourceDir = this.Locations["PackageDir"].SubDirectory("source");
                this.Include(sourceDir, "library_cpp.c");
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_UpdateOptions);
                this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SourceFiles_ExceptionHandling);
            }

            void SourceFiles_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICCompilerOptions;
                compilerOptions.IncludePaths.Include(this.Locations["PackageDir"], "include");
            }

            void SourceFiles_ExceptionHandling(Opus.Core.IModule module, Opus.Core.Target target)
            {
                var compilerOptions = module.Options as C.ICxxCompilerOptions;
                compilerOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;
            }
        }

        [Opus.Core.SourceFiles]
        SourceFiles sourceFiles = new SourceFiles();

        [Opus.Core.DependentModules(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.TypeArray winVCDependents = new Opus.Core.TypeArray(typeof(WindowsSDK.WindowsSDK));

        [C.RequiredLibraries(Platform = Opus.Core.EPlatform.Windows, ToolsetTypes = new[] { typeof(VisualC.Toolset) })]
        Opus.Core.StringArray libraries = new Opus.Core.StringArray("KERNEL32.lib");
    }
}
