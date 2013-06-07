namespace Clide
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.ComponentModelHost;
    using System.IO;
    using System.Xml;
    using System.ComponentModel.Composition.Primitives;
    using Clide;
    using Microsoft.VisualStudio.Text.Editor;
    using IntegrationPackage;
    using System.Dynamic;
    using System.Diagnostics;
    using Clide.Composition;
    using System.Xml.Linq;

    [TestClass]
    public class HostingSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenLoadingBaseDirectory_ThenWorksOnBoth()
        {
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingAPackageExport_ThenReturnsSingleExport()
        {
            var devEnv = DevEnv.Get(new Guid(Constants.PackageGuid));

            var models = devEnv.ServiceLocator.GetAllInstances<ViewModel>().ToList();

            Assert.Equal(1, models.Count);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingShellPackage_ThenSucceeds()
        {
            var components = ServiceProvider.GetService<SComponentModel, IComponentModel>();
            var package = components.GetService<IShellComponent>();

            Assert.NotNull(package);

            package = ServiceLocator.GetInstance<IShellComponent>();

            Assert.NotNull(package);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenExportingVsAdornmentFactory_ThenCanRetrieveIt()
        {
            var components = ServiceProvider.GetService<SComponentModel, IComponentModel>();
            var factory = components.DefaultExportProvider.GetExports<IWpfTextViewCreationListener, IDictionary<string, object>>()
                .Where(e => e.Metadata.ContainsKey("IsClide"))
                .FirstOrDefault();

            Assert.NotNull(factory);
        }
    }
}