#!/usr/bin/python

import os
import sys
from optparse import OptionParser

sys.path.append("../../../../python")
from executeprocess import ExecuteProcess
from getpaths import GetOpusPaths
from standardoptions import StandardOptions

parser = OptionParser()
(options, args, extra_args) = StandardOptions(parser)

opusPackageDir, opusTestPackageDir, opusCodeGeneratorExe = GetOpusPaths()

# C compiler options
cCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICCompilerOptions.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualCCommon", "dev", "Scripts", "ICCompilerOptions.cs")),
    "-n=VisualCCommon",
    "-c=CCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualStudioProcessor", "dev", "Scripts", "VisualStudioDelegate.cs")),
    "-pv=PrivateData"
]
cCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cCompiler_options, True, True)
print stdout

# C++ compiler options
cxxCompiler_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=VisualCCommon",
    "-c=CxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualStudioProcessor", "dev", "Scripts", "VisualStudioDelegate.cs")),
    "-pv=PrivateData",
    "-e" # this option set derives from the C option set
]
cxxCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cxxCompiler_options, True, True)
print stdout

# Linker options
linker_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "ILinkerOptions.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualCCommon", "dev", "Scripts", "ILinkerOptions.cs")),
    "-n=VisualCCommon",
    "-c=LinkerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualStudioProcessor", "dev", "Scripts", "VisualStudioDelegate.cs")),
    "-pv=PrivateData"
]
linker_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(linker_options, True, True)
print stdout

# Archiver options
archiver_options = [
    opusCodeGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(opusPackageDir, "C", "dev", "Scripts", "IArchiverOptions.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualCCommon", "dev", "Scripts", "IArchiverOptions.cs")),
    "-n=VisualCCommon",
    "-c=ArchiverOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(opusPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")) + os.pathsep + os.path.relpath(os.path.join(opusPackageDir, "VisualStudioProcessor", "dev", "Scripts", "VisualStudioDelegate.cs")),
    "-pv=PrivateData"
]
archiver_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(archiver_options, True, True)
print stdout
