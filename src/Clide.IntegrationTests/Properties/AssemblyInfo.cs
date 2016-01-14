using System.Reflection;
using Xunit;

[assembly: AssemblyTitle ("Clide.IntegrationTests")]
[assembly: AssemblyDescription ("Clide.IntegrationTests")]

#if DEBUG
[assembly: VsixRunner(ProcessStartRetries = 1, RemoteConnectionRetries = 1, DebuggerAttachRetries = 5, StartupTimeout = 300)]
#endif

#if CI
[assembly: Vsix (VisualStudioVersion.All, MinimumVisualStudioVersion = VisualStudioVersion.VS2012, RootSuffix = "")]
#else
// By not specifying VisualStudioVersion.All, the tests will run by default with the current version 
// or the latest installed if run from a command line runner.
[assembly: Vsix (MinimumVisualStudioVersion = VisualStudioVersion.VS2012, RootSuffix = "Exp")]
#endif