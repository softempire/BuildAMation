#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using Bam.Core;
namespace Publisher
{
    public abstract class Collation :
        Bam.Core.Module
    {
        private ICollationPolicy Policy = null;
        public static Bam.Core.FileKey PackageRoot = Bam.Core.FileKey.Generate("Package Root");

        public enum EPublishingType
        {
            ConsoleApplication,
            WindowedApplication
        }

        protected Collation()
        {
            this.RegisterGeneratedFile(PackageRoot, this.CreateTokenizedString("$(buildroot)/$(modulename)-$(config)"));
        }

        private string
        PublishingPath(
            Bam.Core.Module module,
            EPublishingType type)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX) &&
                (EPublishingType.WindowedApplication == type))
            {
                return module.CreateTokenizedString("$(OutputName).app/Contents/MacOS").Parse();
            }
            return null;
        }

        private CollatedFile
        CreateCollatedFile(
            CollatedFile reference = null,
            Bam.Core.TokenizedString subDirectory = null)
        {
            var copyFileModule = Bam.Core.Module.Create<CollatedFile>(preInitCallback: module =>
                {
                    if (reference != null)
                    {
                        if (null != subDirectory)
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey], subDirectory);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey]);
                        }
                    }
                    else
                    {
                        if (null != subDirectory)
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/$(1)/)", this.GeneratedPaths[PackageRoot], subDirectory);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/)", this.GeneratedPaths[PackageRoot]);
                        }
                    }
                });
            this.Requires(copyFileModule);
            if (null != reference)
            {
                copyFileModule.Reference = reference;
            }
            if (null != subDirectory)
            {
                copyFileModule.SubDirectory = subDirectory;
            }
            return copyFileModule;
        }

        private CollatedDirectory
        CreateCollatedDirectory(
            CollatedFile reference = null,
            Bam.Core.TokenizedString subDirectory = null)
        {
            var copyDirectoryModule = Bam.Core.Module.Create<CollatedDirectory>(preInitCallback: module =>
            {
                // Windows XCOPY requires the directory name to be added to the destination, while Posix cp does not
                if (reference != null)
                {
                    if (null != subDirectory)
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/$(1)/@filename($(2))/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey], subDirectory, (module as CollatedDirectory).SourcePath);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey], subDirectory);
                        }
                    }
                    else
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/@filename($(1))/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey], (module as CollatedDirectory).SourcePath);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey]);
                        }
                    }
                }
                else
                {
                    if (null != subDirectory)
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/$(1)/@filename($(2))/)", this.GeneratedPaths[PackageRoot], subDirectory, (module as CollatedDirectory).SourcePath);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/$(1)/)", this.GeneratedPaths[PackageRoot], subDirectory);
                        }
                    }
                    else
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/@filename($(1))/)", this.GeneratedPaths[PackageRoot], (module as CollatedDirectory).SourcePath);
                        }
                        else
                        {
                            module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/)", this.GeneratedPaths[PackageRoot]);
                        }
                    }
                }
            });
            this.Requires(copyDirectoryModule);
            if (null != reference)
            {
                copyDirectoryModule.Reference = reference;
            }
            if (null != subDirectory)
            {
                copyDirectoryModule.SubDirectory = subDirectory;
            }
            return copyDirectoryModule;
        }

        private CollatedSymbolicLink
        CreateCollatedSymbolicLink(
            CollatedFile reference = null,
            Bam.Core.TokenizedString subDirectory = null)
        {
            var copySymlinkModule = Bam.Core.Module.Create<CollatedSymbolicLink>(preInitCallback: module =>
            {
                if (reference != null)
                {
                    if (null != subDirectory)
                    {
                        module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/$(1)/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey], subDirectory);
                    }
                    else
                    {
                        module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize(@dir($(0))/)", reference.GeneratedPaths[CollatedObject.CopiedObjectKey]);
                    }
                }
                else
                {
                    if (null != subDirectory)
                    {
                        module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/$(1)/)", this.GeneratedPaths[PackageRoot], subDirectory);
                    }
                    else
                    {
                        module.Macros["CopyDir"] = this.CreateTokenizedString("@normalize($(0)/)", this.GeneratedPaths[PackageRoot]);
                    }
                }
            });
            this.Requires(copySymlinkModule);
            if (null != reference)
            {
                copySymlinkModule.Reference = reference;
            }
            if (null != subDirectory)
            {
                copySymlinkModule.SubDirectory = subDirectory;
            }
            return copySymlinkModule;
        }

        public CollatedFile
        Include<DependentModule>(
            Bam.Core.FileKey key,
            EPublishingType type,
            string subdir = null) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            var path = this.PublishingPath(dependent, type);
            string destSubDir;
            if (null == path)
            {
                destSubDir = subdir;
            }
            else
            {
                if (null != subdir)
                {
                    destSubDir = System.IO.Path.Combine(path, subdir);
                }
                else
                {
                    destSubDir = path;
                }
            }

            var copyFileModule = this.CreateCollatedFile(subDirectory: Bam.Core.TokenizedString.CreateVerbatim(destSubDir));
            copyFileModule.SourceModule = dependent;
            copyFileModule.SourcePath = dependent.GeneratedPaths[key];

            return copyFileModule;
        }

        public void
        Include<DependentModule>(
            Bam.Core.FileKey key,
            string subdir,
            CollatedFile reference) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            var copyFileModule = this.CreateCollatedFile(reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));
            copyFileModule.SourceModule = dependent;
            copyFileModule.SourcePath = dependent.GeneratedPaths[key];
        }

        public void
        IncludeFiles<DependentModule>(
            string parameterizedFilePath,
            string subdir,
            CollatedFile reference) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            var copyFileModule = this.CreateCollatedFile(reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));
            copyFileModule.SourceModule = dependent;
            copyFileModule.SourcePath = dependent.CreateTokenizedString(parameterizedFilePath);
        }

        public void
        IncludeFile(
            string parameterizedFilePath,
            string subdir,
            CollatedFile reference)
        {
            var tokenString = this.CreateTokenizedString(parameterizedFilePath);
            this.IncludeFile(tokenString, subdir, reference);
        }

        public void
        IncludeFile(
            Bam.Core.TokenizedString parameterizedFilePath,
            string subdir,
            CollatedFile reference)
        {
            var copyFileModule = this.CreateCollatedFile(reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));
            copyFileModule.SourcePath = parameterizedFilePath;
        }

        public void
        IncludeDirectory(
            Bam.Core.TokenizedString parameterizedPath,
            string subdir,
            CollatedFile reference)
        {
            var copyDirectoryModule = this.CreateCollatedDirectory(reference, Bam.Core.TokenizedString.CreateVerbatim(subdir));
            copyDirectoryModule.SourcePath = parameterizedPath;
        }

        public void
        IncludeFramework<DependentModule>(
            string subdir,
            CollatedFile reference) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            var subdirTS = Bam.Core.TokenizedString.CreateVerbatim(subdir);

            var framework = dependent as C.ExternalFramework;
            if (null == framework)
            {
                throw new Bam.Core.Exception("Module {0} did not derive from {1}", dependent.GetType().ToString(), typeof(C.ExternalFramework).ToString());
            }
            var frameworkPath = framework.FrameworkPath;

            var dirPublishedModules = new Bam.Core.Array<CollatedDirectory>();
            if (null != framework.DirectoriesToPublish)
            foreach (var dir in framework.DirectoriesToPublish)
            {
                var copyDir = this.CreateCollatedDirectory(reference, this.CreateTokenizedString("$(0)/$(1)", subdirTS, dir));
                copyDir.SourceModule = dependent;
                copyDir.SourcePath = this.CreateTokenizedString("$(0)/$(1)", frameworkPath, dir);
                dirPublishedModules.Add(copyDir);
            }
            var filePublishedModules = new Bam.Core.Array<CollatedFile>();
            if (null != framework.FilesToPublish)
            {
                foreach (var file in framework.FilesToPublish)
                {
                    var copyFile = this.CreateCollatedFile(reference, this.CreateTokenizedString("$(0)/@dir($(1))", subdirTS, file));
                    copyFile.SourceModule = dependent;
                    copyFile.SourcePath = this.CreateTokenizedString("$(0)/$(1)", frameworkPath, file);
                    foreach (var publishedDir in dirPublishedModules)
                    {
                        copyFile.Requires(publishedDir);
                    }
                    filePublishedModules.Add(copyFile);

                    if (file == framework.Macros["FrameworkLibraryPath"])
                    {
                        var updateIDName = Bam.Core.Module.Create<IdNameOSX>();
                        updateIDName.Source = copyFile;
                        this.Requires(updateIDName);
                    }
                }
            }
            if (null != framework.SymlinksToPublish)
            {
                foreach (var symlink in framework.SymlinksToPublish)
                {
                    var copySymlink = this.CreateCollatedSymbolicLink(reference, this.CreateTokenizedString("$(0)/@dir($(1))", subdirTS, symlink));
                    copySymlink.SourceModule = dependent;
                    copySymlink.SourcePath = this.CreateTokenizedString("$(0)/$(1)", frameworkPath, symlink);
                    foreach (var publishedDir in dirPublishedModules)
                    {
                        copySymlink.Requires(publishedDir);
                    }
                    foreach (var publishedFile in filePublishedModules)
                    {
                        copySymlink.Requires(publishedFile);
                    }
                }
            }
        }

        public override void
        Evaluate()
        {
            // TODO
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }
            this.Policy.Collate(this, context);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
                case "MakeFile":
                    {
                        var className = "Publisher." + mode + "Collation";
                        this.Policy = Bam.Core.ExecutionPolicyUtilities<ICollationPolicy>.Create(className);
                    }
                    break;
            }
        }
    }
}
