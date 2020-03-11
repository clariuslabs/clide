using System.Reflection;
using Xunit;

[assembly: AssemblyDescription("Clide.Windows.IntegrationTests")]

#if DEBUG
[assembly: VsixRunner(ProcessStartRetries = 1, RemoteConnectionRetries = 1, DebuggerAttachRetries = 5, StartupTimeout = 300)]
#endif

#if DEBUG
// Limit run to current VS version
[assembly: Vsix(ThisAssembly.Metadata.VisualStudioVersion,
    MinimumVisualStudioVersion = ThisAssembly.Metadata.VisualStudioVersion,
    MaximumVisualStudioVersion = ThisAssembly.Metadata.VisualStudioVersion,
    TimeoutSeconds = 120,
    // No need to recycle on local dev runs
    RecycleOnFailure = false)]
#else
[assembly: Vsix("16.0", MinimumVisualStudioVersion = "16.0", TimeoutSeconds = 120, RecycleOnFailure = true)]
#endif
