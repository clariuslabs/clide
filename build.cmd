@echo off
rem Only need to run this the first time after clone. Subsequent builds can be just "msbuild" or "xbuild".

cd %~dp0

SETLOCAL
SET CACHED_NUGET=%LocalAppData%\NuGet\NuGet.exe

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST src\.nuget\nuget.exe goto restore
md src\.nuget
copy %CACHED_NUGET% src\.nuget\nuget.exe > nul

:restore
src\.nuget\NuGet.exe install build\packages.config -OutputDirectory build\packages -ExcludeVersion

:run
msbuild build.proj /v:detailed %1 %2 %3 %4 %5 %6 %7 %8 %9