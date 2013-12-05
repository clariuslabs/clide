namespace Clide
{
    using EnvDTE;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Linq;
    using Microsoft.VisualStudio.Language.Intellisense;
    using System.Collections.Generic;
    using System;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.VisualStudio.Shell;

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

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingExportedMultipleComponents_ThenSucceeds()
        {
            var devEnv = DevEnv.Get(this.ServiceProvider);
            var result = devEnv.ServiceLocator.GetAllInstances<ICompletionSourceProvider>().ToList();

            Assert.True(result.Any());
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingSingleComponent_ThenSucceeds()
        {
            var devEnv = DevEnv.Get(GlobalServiceProvider.Instance);

            Assert.NotNull(devEnv.ServiceLocator.GetInstance<ISmartTagBroker>());
        }
	}
}
