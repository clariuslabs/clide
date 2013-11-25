namespace Clide
{
    using EnvDTE;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Linq;
    using Microsoft.VisualStudio.Language.Intellisense;
    using System.Collections.Generic;
    using Autofac.Core;
    using System;
    using Microsoft.Practices.ServiceLocation;

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
        public void WhenRetrievingExportedMultipleComponents_ThenThrowsNotSupported()
        {
            var devEnv = DevEnv.Get(GlobalServiceProvider.Instance);
            var ex = Assert.Throws<ActivationException>(() => devEnv.ServiceLocator.GetAllInstances<ICompletionSourceProvider>().ToList());

            Assert.True(ex.InnerException is DependencyResolutionException);
            Assert.True(((DependencyResolutionException)ex.InnerException).InnerException is NotSupportedException);
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
