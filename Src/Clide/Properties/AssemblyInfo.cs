using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Clide")]
[assembly: AssemblyDescription("Provides high-level, composable and testable APIs for working with Visual Studio.")]
[assembly: AssemblyCompany("Clarius Consulting")]
[assembly: AssemblyProduct("Clide")]
[assembly: AssemblyCopyright("Copyright 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("84d7eb01-bbad-4bd0-8c70-cd5720e33f8d")]

[assembly: AssemblyVersion("1.1.1212.1411")]
[assembly: AssemblyFileVersion("1.1.1212.1411")]

[assembly: InternalsVisibleTo("Clide.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d9776e258bdf5f7c38f3c880404b9861ebbd235d8198315cdfda0f0c25b18608bdfd03e34bac9d0ec95766e8c3928140c6eda581a9448066af7dfaf88d3b6cb71d45a094011209ff6e76713151b4f2ce469cd2886285f1bf565b7fa63dada9d2e9573b743d26daa608b4d0fdebc9daa907a52727448316816f9c05c6e5529b9f")]
[assembly: InternalsVisibleTo("Clide.IntegrationTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d9776e258bdf5f7c38f3c880404b9861ebbd235d8198315cdfda0f0c25b18608bdfd03e34bac9d0ec95766e8c3928140c6eda581a9448066af7dfaf88d3b6cb71d45a094011209ff6e76713151b4f2ce469cd2886285f1bf565b7fa63dada9d2e9573b743d26daa608b4d0fdebc9daa907a52727448316816f9c05c6e5529b9f")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2,PublicKey=0024000004800000940000000602000000240000525341310004000001000100d9776e258bdf5f7c38f3c880404b9861ebbd235d8198315cdfda0f0c25b18608bdfd03e34bac9d0ec95766e8c3928140c6eda581a9448066af7dfaf88d3b6cb71d45a094011209ff6e76713151b4f2ce469cd2886285f1bf565b7fa63dada9d2e9573b743d26daa608b4d0fdebc9daa907a52727448316816f9c05c6e5529b9f")]

[assembly: AssemblyConfiguration(ThisAssembly.Configuration)]
internal class ThisAssembly
{
#if DEBUG
    public const string Configuration = "DEBUG";
#else    
    public const string Configuration = "RELEASE";
#endif
}
