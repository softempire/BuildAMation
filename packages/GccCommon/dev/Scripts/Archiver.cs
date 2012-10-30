// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public class Archiver : C.Archiver, Opus.Core.ITool
    {
        private string binPath;

        public Archiver(Opus.Core.Target target)
        {
            if (!(Opus.Core.OSUtilities.IsUnix(target) || Opus.Core.OSUtilities.IsOSX(target)))
            {
                throw new Opus.Core.Exception("Gcc archiver is only supported under unix32, unix64, osx32 and osx64 platforms");
            }

            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Gcc.Toolset));
            this.binPath = info.BinPath((Opus.Core.BaseTarget)target);
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);
#endif
        }

        public string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "ar");
        }
    }
}

