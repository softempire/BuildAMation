// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed partial class CxxCompilerOptionCollection :
        CCompilerOptionCollection,
        C.ICxxCompilerOptions
    {
        public
        CxxCompilerOptionCollection(
            Opus.Core.DependencyNode owningNode) : base(owningNode)
        {}

        protected override void
        SetDefaultOptionValues(
            Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var cInterfaceOptions = this as C.ICCompilerOptions;
            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.Cxx;

            var cxxInterfaceOptions = this as C.ICxxCompilerOptions;
            cxxInterfaceOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Disabled;
        }
    }
}