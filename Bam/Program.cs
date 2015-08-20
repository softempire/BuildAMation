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
namespace Bam
{
    /// <summary>
    /// Command line tool main entry point
    /// </summary>
    class Program
    {
        static bool UseV2 = true;

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Argument array.</param>
        static void
        Main(
            string[] args)
        {
            if (UseV2)
            {
                var createDebugProject = Core.V2.CommandLineProcessor.Evaluate(new Core.V2.CreateDebugProject());
                if (createDebugProject)
                {
                    V2.DebugProject.Create();
                    return;
                }

                // configure
                Core.State.BuildRoot = "build";
                Core.State.VerbosityLevel = Core.EVerboseLevel.Full;
                Core.State.CompileWithDebugSymbols = true;
                Core.State.BuilderName = Core.V2.CommandLineProcessor.Evaluate(new Core.V2.BuilderName());
                if (null == Core.State.BuilderName)
                {
                    throw new Core.Exception("No builder specified");
                }

                var debug = new Core.V2.Environment();
                debug.Configuration = Core.EConfiguration.Debug;

                var optimized = new Core.V2.Environment();
                optimized.Configuration = Core.EConfiguration.Optimized;

                Core.V2.EntryPoint.Execute(new Core.Array<Core.V2.Environment>(debug/*, optimized*/));
            }
            else
            {
                // take control of Ctrl+C
                System.Console.CancelKeyPress += new System.ConsoleCancelEventHandler(HandleCancellation);

                try
                {
                    var profile = new Core.TimeProfile(Core.ETimingProfiles.TimedTotal);
                    profile.StartProfile();

                    var application = new Application(args);
                    application.Run();

                    profile.StopProfile();

                    if (Core.State.ShowTimingStatistics)
                    {
                        Core.TimingProfileUtilities.DumpProfiles();
                    }
                }
                catch (Core.Exception exception)
                {
                    Core.Exception.DisplayException(exception);
                    System.Environment.ExitCode = -1;
                }
                catch (System.Reflection.TargetInvocationException exception)
                {
                    Core.Exception.DisplayException(exception);
                    System.Environment.ExitCode = -2;
                }
                catch (System.Exception exception)
                {
                    Core.Exception.DisplayException(exception);
                    System.Environment.ExitCode = -3;
                }
                finally
                {
                    if (0 == System.Environment.ExitCode)
                    {
                        Core.Log.Info("\nSucceeded");
                    }
                    else
                    {
                        Core.Log.Info("\nFailed");
                    }
                    Core.Log.DebugMessage("Exit code is {0}", System.Environment.ExitCode);
                }
            }
        }

        private static void
        HandleCancellation(
            object sender,
            System.ConsoleCancelEventArgs e)
        {
            // allow the build to fail gracefully
            var buildManager = Core.State.BuildManager;
            if (null != buildManager)
            {
                buildManager.Cancel();
                e.Cancel = true;
            }
        }
    }
}