using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clide
{
	[TestClass]
	public class VsExposedServicesSpec : VsHostedSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[HostType("VS IDE")]
		[TestMethod]
		public void WhenGettingExportedServices_ThenSuccceedsForAll()
		{
            var devEnv = DevEnv.Get(GlobalServiceProvider.Instance);

            Assert.NotNull(devEnv.ServiceLocator.GetInstance<DTE>());
            Assert.NotNull(devEnv.ServiceLocator.GetInstance<IVsShell>());
		}
	}
}
