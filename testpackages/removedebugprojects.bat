@ECHO OFF

FOR /D %%A IN (%CD%\*) DO CALL :FIND_PACKAGE_VERSIONS %%A
PAUSE
GOTO :EOF

:FIND_PACKAGE_VERSIONS
FOR /D %%B IN (%1\*) DO CALL :DELETE_OPUS_DIRECTORY %%B
FOR /D %%B IN (%1\*) DO CALL :DELETE_BUILD_DIRECTORY %%B
GOTO :EOF

:DELETE_OPUS_DIRECTORY
IF EXIST %1\Opus (
    ECHO Deleting '%1\Opus' directory and all children
    RMDIR /S /Q %1\Opus
)

:DELETE_BUILD_DIRECTORY
IF EXIST %1\build (
    ECHO Deleting '%1\build' directory and all children
    RMDIR /S /Q %1\build
)
