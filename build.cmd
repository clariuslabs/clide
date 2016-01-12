@echo off
rem Optional batch file to quickly build with some defaults.
rem Alternatively, this batch file can be invoked passing msbuild parameters, like: build.cmd /v:detailed /t:Rebuild

cd %~dp0

SETLOCAL
SET CACHED_NUGET=%LocalAppData%\NuGet\NuGet.exe

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST .nuget\nuget.exe goto restore
md .nuget
copy %CACHED_NUGET% .nuget\nuget.exe > nul
.nuget\nuget.exe update -self

:restore
rem Build script packages have no version in the path, so we install them to .nuget\packages to avoid conflicts with 
rem solution/project packages.
IF NOT EXIST packages.config goto run
.nuget\NuGet.exe install packages.config -OutputDirectory .nuget\packages -ExcludeVersion

:run
rem NOTE: this needs to be a developer command prompt with MSBuild in the path.
msbuild build.proj /t:build,test,package /v:normal %1 %2 %3 %4 %5 %6 %7 %8 %9