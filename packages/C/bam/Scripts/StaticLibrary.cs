#region License
// Copyright (c) 2010-2016, Mark Final
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
namespace C
{
    /// <summary>
    /// Derive from this module to create a static library of object files, in any language.
    /// </summary>
    public class StaticLibrary :
        CModule,
        IForwardedLibraries
    {
        private Bam.Core.Array<Bam.Core.Module> sourceModules = new Bam.Core.Array<Bam.Core.Module>();
        private Bam.Core.Array<Bam.Core.Module> forwardedDeps = new Bam.Core.Array<Bam.Core.Module>();
        private IArchivingPolicy Policy = null;

        static public Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Static Library File");

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Librarian = DefaultToolchain.Librarian(this.BitDepth);
            this.RegisterGeneratedFile(Key, this.CreateTokenizedString("$(packagebuilddir)/$(moduleoutputdir)/$(libprefix)$(OutputName)$(libext)"));
        }

        // TODO: what is this for?
        public System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> Source
        {
            get
            {
                return this.sourceModules.ToReadOnlyCollection();
            }
        }

        System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> IForwardedLibraries.ForwardedLibraries
        {
            get
            {
                return this.forwardedDeps.ToReadOnlyCollection();
            }
        }

        public CObjectFileCollection
        CreateCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<CObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter);
            this.sourceModules.Add(source);
            return source;
        }

        public Cxx.ObjectFileCollection
        CreateCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<Cxx.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter);
            this.sourceModules.Add(source);
            return source;
        }

        public C.ObjC.ObjectFileCollection
        CreateObjectiveCSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<C.ObjC.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter);
            this.sourceModules.Add(source);
            return source;
        }

        public C.ObjCxx.ObjectFileCollection
        CreateObjectiveCxxSourceContainer(
            string wildcardPath = null,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var source = this.InternalCreateContainer<C.ObjCxx.ObjectFileCollection>(false, wildcardPath, macroModuleOverride, filter);
            this.sourceModules.Add(source);
            return source;
        }

        public void
        CompileAgainst<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            if (0 == affectedSources.Length)
            {
                throw new Bam.Core.Exception("At least one source module argument must be passed to {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, this.ToString());
            }

            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            foreach (var source in affectedSources)
            {
                if (null == source)
                {
                    continue;
                }
                source.UsePublicPatches(dependent);
            }
            if (dependent is HeaderLibrary)
            {
                this.Requires(dependent);
            }
            else
            {
                // this delays the dependency until a link
                this.forwardedDeps.AddUnique(dependent);
            }
        }

        public void
        CompileAgainstPublicly<DependentModule>(
            params CModule[] affectedSources) where DependentModule : CModule, new()
        {
            this.CompileAgainst<DependentModule>(affectedSources);
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.UsePublicPatches(dependent);
        }

        public LibrarianTool Librarian
        {
            get
            {
                return this.Tool as LibrarianTool;
            }
            set
            {
                this.Tool = value;
            }
        }

        protected sealed override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            var source = FlattenHierarchicalFileList(this.sourceModules).ToReadOnlyCollection();
            var headers = FlattenHierarchicalFileList(this.headerModules).ToReadOnlyCollection();
            var libraryFile = this.GeneratedPaths[Key];
            this.Policy.Archive(this, context, libraryFile, source, headers);
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
            var className = "C." + mode + "Librarian";
            this.Policy = Bam.Core.ExecutionPolicyUtilities<IArchivingPolicy>.Create(className);
        }

        public sealed override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var exists = System.IO.File.Exists(this.GeneratedPaths[Key].ToString());
            if (!exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            foreach (var source in this.sourceModules)
            {
                if (null != source.EvaluationTask)
                {
                    source.EvaluationTask.Wait();
                }
                if (null != source.ReasonToExecute)
                {
                    this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], source.ReasonToExecute.OutputFilePath);
                    return;
                }
            }
        }
    }
}
