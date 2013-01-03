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
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingPackage_ThenSucceeds()
        {
            var package = ServiceProvider.GetExportedValue<IShellComponent>();
            
            Assert.NotNull(package);
        }
    }
}
