using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Clide
{
    [TestClass]
    public class GlobalServiceProviderSpec
    {
        [HostType("VS IDE")]
        [TestMethod]
        public void WhenGettingGlobalProvider_ThenCanGetInstanceVsServices()
        {
            Assert.IsNotNull(GlobalServiceProvider.Instance.GetService<SVsShell, IVsShell>());
            Assert.IsNotNull(GlobalServiceProvider.Instance.GetService<DTE>());
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenUsingGlobalProvider_ThenCanGetComponentModel()
        {
            // NOTE: SVsServiceProvider is an export, not a service, so it can't be retrieved from the global provider.
            Assert.IsNotNull(GlobalServiceProvider.Instance.GetService<SComponentModel, IComponentModel>());
        }
    }
}