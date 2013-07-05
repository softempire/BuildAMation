// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder2
    {
        public object Build(C.ObjectFile moduleToBuild, out bool success)
        {
            var sourceFilePath = moduleToBuild.SourceFile.AbsolutePath;

            // any source file that is derived from a moc step should not be listed
            // explicitly, because QMake handles this via searching for Q_OBJECT classes
            if (System.IO.Path.GetFileNameWithoutExtension(sourceFilePath).StartsWith("moc_"))
            {
                success = true;
                return null;
            }

            var node = moduleToBuild.OwningNode;
            var options = moduleToBuild.Options as C.CompilerOptionCollection;
            var optionInterface = moduleToBuild.Options as C.ICCompilerOptions;

            var data = new QMakeData(node);
            data.PriPaths.Add(this.EmptyConfigPriPath);
            data.Sources.Add(sourceFilePath);
            data.Output = QMakeData.OutputType.ObjectFile;
            data.ObjectsDir = options.OutputDirectoryPath;
            data.IncludePaths.AddRangeUnique(optionInterface.IncludePaths.ToStringArray());
            data.Defines.AddRangeUnique(optionInterface.Defines.ToStringArray());

            success = true;
            return data;
        }
    }

    public sealed partial class QMakeBuilder
    {
        public object Build(C.ObjectFile objectFile, out bool success)
        {
            // any source file generated by moc should not be included in the .pro file (QMake handles this)
            string sourceFilePath = objectFile.SourceFile.AbsolutePath;
            // cannot assume QtCommon is a dependency in the project, so can't use QtCommon.MocFile.Prefix
            // (TODO: there is an issue adding QtCommon as a dependency to QMakeBuilder to do with the moc version)
            if (System.IO.Path.GetFileNameWithoutExtension(sourceFilePath).StartsWith("moc_"))
            {
                success = true;
                return null;
            }

            var objectFileModule = objectFile as Opus.Core.BaseModule;
            var target = objectFileModule.OwningNode.Target;
            var objectFileOptions = objectFileModule.Options;

            var compilerOptions = objectFileOptions as C.CompilerOptionCollection;
            var commandLineBuilder = new Opus.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            var nodeData = new NodeData();
            nodeData.Configuration = GetQtConfiguration(target);
            nodeData.AddVariable("SOURCES", sourceFilePath);
            if (objectFileOptions is C.ICxxCompilerOptions)
            {
                var compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool));
                nodeData.AddUniqueVariable("CXXFLAGS", commandLineBuilder);
                nodeData.AddUniqueVariable("QMAKE_CXX", new Opus.Core.StringArray(compilerTool.Executable((Opus.Core.BaseTarget)target).Replace("\\", "/")));
            }
            else
            {
                var compilerTool = target.Toolset.Tool(typeof(C.ICxxCompilerTool));
                nodeData.AddUniqueVariable("CFLAGS", commandLineBuilder);
                nodeData.AddUniqueVariable("QMAKE_CC", new Opus.Core.StringArray(compilerTool.Executable((Opus.Core.BaseTarget)target).Replace("\\", "/")));
            }
            nodeData.AddUniqueVariable("OBJECTS_DIR", new Opus.Core.StringArray(compilerOptions.OutputDirectoryPath));

            success = true;
            return nodeData;
        }
    }
}