using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using System.ComponentModel.Composition.Hosting;
using Clide.Composition;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

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
