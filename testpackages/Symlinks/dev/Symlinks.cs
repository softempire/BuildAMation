// Automatically generated by Opus v0.40
namespace Symlinks
{
#if OPUSPACKAGE_FILEUTILITIES_DEV
    class SymLinkToFile : FileUtilities.SymlinkFile
    {
        public SymLinkToFile()
        {
            var dataDir = this.Locations["PackageDir"].SubDirectory("data");
            this.Include(dataDir, "TestFile.txt");
        }
    }

    class SymLinkToFileRenamed : FileUtilities.SymlinkFile
    {
        public SymLinkToFileRenamed()
        {
            var dataDir = this.Locations["PackageDir"].SubDirectory("data");
            this.Include(dataDir, "TestFile.txt");
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var options = module.Options as FileUtilities.ISymlinkOptions;
                options.TargetName = "RenamedLinkedFile.txt";
            };
        }
    }

    class SymLinkToFileNextTo : FileUtilities.SymlinkFile
    {
        public SymLinkToFileNextTo()
        {
            var dataDir = this.Locations["PackageDir"].SubDirectory("data");
            this.Include(dataDir, "TestFile.txt");
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var options = module.Options as FileUtilities.ISymlinkOptions;
                options.TargetName = "NextToLinkedFile.txt";
            };
        }

        [FileUtilities.BesideModule(FileUtilities.OutputFileFlags.Symlink)]
        System.Type nextTo = typeof(SymLinkToFile);
    }

    class SymLinkToBuiltFile : FileUtilities.SymlinkFile
    {
        public SymLinkToBuiltFile()
        {
            this.Set(typeof(SymLinkToFile), FileUtilities.OutputFileFlags.Symlink);
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var options = module.Options as FileUtilities.ISymlinkOptions;
                options.TargetName = "LinkedBuiltFile.txt";
            };
        }
    }

    class SymlinkToDirectory : FileUtilities.SymlinkDirectory
    {
        public SymlinkToDirectory()
        {
            var dataDir = this.Locations["PackageDir"].SubDirectory("data");
            this.Include(dataDir, "TestDir");
        }
    }

    class SymlinkToDirectoryRenamed : FileUtilities.SymlinkDirectory
    {
        public SymlinkToDirectoryRenamed()
        {
            var dataDir = this.Locations["PackageDir"].SubDirectory("data");
            this.Include(dataDir, "TestDir");
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var options = module.Options as FileUtilities.ISymlinkOptions;
                options.TargetName = "RenamedLinkedDir";
            };
        }
    }

    class SymlinkToDirectoryNextTo : FileUtilities.SymlinkDirectory
    {
        public SymlinkToDirectoryNextTo()
        {
            var dataDir = this.Locations["PackageDir"].SubDirectory("data");
            this.Include(dataDir, "TestDir");
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var options = module.Options as FileUtilities.ISymlinkOptions;
                options.TargetName = "NextToLinkedDir";
            };
        }

        [FileUtilities.BesideModule(FileUtilities.OutputFileFlags.Symlink)]
        System.Type nextTo = typeof(SymlinkToDirectory);
    }

    // this one is a bit unusual
    class SymlinkToBuiltDir : FileUtilities.SymlinkDirectory
    {
        public SymlinkToBuiltDir()
        {
            this.Set(typeof(SymlinkToDirectory), FileUtilities.OutputFileFlags.Symlink);
            this.UpdateOptions += delegate(Opus.Core.IModule module, Opus.Core.Target target) {
                var options = module.Options as FileUtilities.ISymlinkOptions;
                options.TargetName = "LinkedBuiltDir";
            };
        }
    }
#elif OPUSPACKAGE_FILEUTILITIES_1_0
    class SymLinkToFile : FileUtilities.SymLink
    {
        public SymLinkToFile()
        {
            this.targetFile.SetRelativePath(this, "data", "TestFile.txt");
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SymLinkToFile_UpdateOptions);
        }

        void SymLinkToFile_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            FileUtilities.ISymLinkOptions options = module.Options as FileUtilities.ISymLinkOptions;
            if (null != options)
            {
                options.LinkName = "TestFile.txt";
            }
        }

        [Opus.Core.SourceFiles]
        Opus.Core.File targetFile = new Opus.Core.File();
    }

    class SymLinkToDir : FileUtilities.SymLink
    {
        public SymLinkToDir()
        {
            this.targetDir.SetRelativePath(this, "data", "TestDir");
            this.UpdateOptions += new Opus.Core.UpdateOptionCollectionDelegate(SymLinkToDir_UpdateOptions);
        }

        void SymLinkToDir_UpdateOptions(Opus.Core.IModule module, Opus.Core.Target target)
        {
            FileUtilities.ISymLinkOptions options = module.Options as FileUtilities.ISymLinkOptions;
            if (null != options)
            {
                options.LinkName = "TestDir";
                options.Type = FileUtilities.EType.Directory;
            }
        }

        [Opus.Core.SourceFiles]
        Opus.Core.File targetDir = new Opus.Core.File();
    }
#endif
}
