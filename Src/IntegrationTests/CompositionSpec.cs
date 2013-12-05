using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Concurrent;
using Clide.Composition;
using EnvDTE;
using EnvDTE80;

namespace Clide
{
    [TestClass]
    public class CompositionSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingServiceProvider_ThenSucceeds()
        {
            var servicesExports = new ServicesExportProvider(GlobalServiceProvider.Instance);

        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingVsService_ThenCanGetFromCompositionContainer()
        {
            var servicesExports = new ServicesExportProvider(GlobalServiceProvider.Instance);

            var container = new CompositionContainer(servicesExports);

            var shell = container.GetExportedValue<IVsUIShell>();

            Assert.NotNull(shell);

            var shell2 = GlobalServiceProvider.Instance.GetService<SVsUIShell, IVsUIShell>();

            Assert.Same(shell, shell2);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingVsServiceWithVersionNumber_ThenCanGetFromCompositionContainer()
        {
            var servicesExports = new ServicesExportProvider(GlobalServiceProvider.Instance);

            var container = new CompositionContainer(servicesExports);

            var shell = container.GetExportedValue<IVsUIShell4>();

            Assert.NotNull(shell);

            var shell2 = GlobalServiceProvider.Instance.GetService<SVsUIShell, IVsUIShell>();

            Assert.Same(shell, shell2);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingDTE_ThenCanGetFromCompositionContainer()
        {
            var servicesExports = new ServicesExportProvider(GlobalServiceProvider.Instance);

            var container = new CompositionContainer(servicesExports);

            var service1 = container.GetExportedValue<DTE>();

            Assert.NotNull(service1);

            var service2 = GlobalServiceProvider.Instance.GetService<SDTE, DTE>();

            Assert.Same(service1, service2);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingDTE2_ThenCanGetFromCompositionContainer()
        {
            var servicesExports = new ServicesExportProvider(GlobalServiceProvider.Instance);

            var container = new CompositionContainer(servicesExports);

            var service1 = container.GetExportedValue<DTE2>();

            Assert.NotNull(service1);

            var service2 = GlobalServiceProvider.Instance.GetService<SDTE, DTE>();

            Assert.Same(service1, service2);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingExportedValueOrDefaultForMany_ThenThrowsImportCardinalityException()
        {
            var components = GlobalServiceProvider.Instance.GetService<SComponentModel, IComponentModel>();

            // ICompletionSourceProvider has many exports.
            // On VS2010 this throws, but on VS2012+ it succeeds and returns null :S
#if Vs10
            Assert.Throws<ImportCardinalityMismatchException>(() => 
                components.DefaultExportProvider.GetExportedValueOrDefault<ICompletionSourceProvider>());
#endif

#if Vs11 || Vs12
            Assert.Equal<ICompletionSourceProvider>(null, components.DefaultExportProvider.GetExportedValueOrDefault<ICompletionSourceProvider>());
#endif
        }
    }
}
