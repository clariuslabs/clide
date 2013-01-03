using IntegrationPackage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clide
{
    [TestClass]
    public class IntegrationPackageSpec : VsHostedSpec
    {
        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingPackage_ThenSucceeds()
        {
            var package = ServiceProvider.GetExportedValue<IShellComponent>();
            
            Assert.IsNotNull(package);
        }
    }
}
