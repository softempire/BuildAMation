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
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder :
        Bam.Core.IBuilderPostExecute
    {
        #region IBuilderPostExecute Members

        void
        Bam.Core.IBuilderPostExecute.PostExecute(
            Bam.Core.DependencyNodeCollection executedNodes)
        {
            Bam.Core.Log.DebugMessage("PostExecute for QMakeBuilder");

            if (0 == executedNodes.Count)
            {
                Bam.Core.Log.Info("No QMake pro file written as there were no targets generated");
                return;
            }

            // find all nodes with the same unique name
            var similarNodes = new System.Collections.Generic.Dictionary<string, Bam.Core.Array<QMakeData>>();
            foreach (var node in executedNodes)
            {
                if (null == node.Data)
                {
                    Bam.Core.Log.DebugMessage("*** Null data for node {0}", node.UniqueModuleName);
                    continue;
                }

                if (similarNodes.ContainsKey(node.UniqueModuleName))
                {
                    similarNodes[node.UniqueModuleName].Add(node.Data as QMakeData);
                }
                else
                {
                    similarNodes[node.UniqueModuleName] = new Bam.Core.Array<QMakeData>(node.Data as QMakeData);
                }
            }

            foreach (var keyPair in similarNodes)
            {
                Bam.Core.Log.DebugMessage("{0} : {1} nodes", keyPair.Key, keyPair.Value.Count);
                QMakeData.Write(keyPair.Value);
            }

#if false
            var mainPackage = Bam.Core.State.PackageInfo[0];
            var proFileName = mainPackage + ".pro";
            var rootDirectory = Bam.Core.State.BuildRoot;
            var proFilePath = System.IO.Path.Combine(rootDirectory, proFileName);

            // relative paths need a trailing slash to work
            rootDirectory += System.IO.Path.DirectorySeparatorChar;

            using (var proWriter = new System.IO.StreamWriter(proFilePath))
            {
                proWriter.WriteLine("# -- Generated by BuildAMation --");
                proWriter.WriteLine("TEMPLATE = subdirs");
                proWriter.WriteLine("CONFIG += ordered");
                proWriter.WriteLine("SUBDIRS += \\");

                foreach (var collection in similarNodes.Values)
                {
                    var data = collection[0];
                    if (data.ProFilePath != null)
                    {
                        var subDirProjectDir = System.IO.Path.GetDirectoryName(data.ProFilePath) + System.IO.Path.DirectorySeparatorChar;
                        var relativeDir = Bam.Core.RelativePathUtilities.GetPath(subDirProjectDir, rootDirectory);
                        relativeDir = relativeDir.TrimEnd(System.IO.Path.DirectorySeparatorChar);
                        proWriter.WriteLine("\t{0}\\", relativeDir.Replace('\\', '/'));
                    }
                }
            }

            Bam.Core.Log.Info("Successfully created QMake .pro file for package '{0}'\n\t{1}", Bam.Core.State.PackageInfo[0].Name, proFilePath);
#endif
        }

        #endregion
    }
}
