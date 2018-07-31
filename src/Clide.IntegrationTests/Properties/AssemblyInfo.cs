using System.Reflection;
using Xunit;

[assembly: AssemblyDescription("Clide.IntegrationTests")]

#if DEBUG
[assembly: VsixRunner(ProcessStartRetries = 1, RemoteConnectionRetries = 1, DebuggerAttachRetries = 5, StartupTimeout = 300)]
#endif