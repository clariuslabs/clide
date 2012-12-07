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
using Microsoft.VisualStudio.ExtensibilityHosting;
using System.ComponentModel.Composition.Primitives;
using Clide;
using FooPackage;

namespace Clide
{
    [TestClass]
    public class HostingSpec : VsHostedSpec
    {
        [HostType("VS IDE")]
        [TestMethod]
        public void WhenDiagnosingMef_ThenCanGetLog()
        {

        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenLoadingVs_ThenCanExportFilteredContainer()
        {
            var components = ServiceProvider.GetService<SComponentModel, IComponentModel>();
            var globalContainer = (CompositionContainer)components.DefaultExportProvider;

            var container = new CompositionContainer(
                new TypeCatalog(typeof(ClideStuff), typeof(AdornmentStuff)));

            var vsixContainer = VsCompositionContainer.Create(
                new NonClideStuff(container));

            try
            {
                var ga1 = globalContainer.GetExportedValue<IAdornmentStuff>();
                Assert.IsNotNull(ga1);
                Assert.IsNotNull(ga1.Clide);
            }
            catch (ImportCardinalityMismatchException)
            {
                Assert.Fail("Did not find IAdornmentStuff on global VS container.");
            }

            try
            {
                globalContainer.GetExportedValue<IClideStuff>();
                Assert.Fail("Should not find IClideStuff on global container.");
            }
            catch (ImportCardinalityMismatchException)
            {
            }
        }

        private class NonClideStuff : ExportProvider
        {
            private ExportProvider provider;

            public NonClideStuff(ExportProvider innerProvider)
            {
                this.provider = innerProvider;
            }

            protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
            {
                IEnumerable<Export> exports;
                this.provider.TryGetExports(definition, atomicComposition, out exports);

                exports = exports.ToList();

                return exports.Where(e => !e.Metadata.ContainsKey("ExportTypeIdentity") || 
                    !e.Metadata["ExportTypeIdentity"].ToString().StartsWith("Clide"));
            }
        }

        //[HostType("VS IDE")]
        //[TestMethod]
        //[Ignore]
        //public void WhenLoadingVs_ThenCanExportNewCatalog()
        //{
        //    // NONE OF THIS CRAP WORKS

        //    var components = ServiceProvider.GetService<SComponentModel, IComponentModel>();
        //    var globalContainer = (CompositionContainer)components.DefaultExportProvider;

        //    var id = Guid.NewGuid();
        //    //globalContainer.ComposeExportedValue<IClideStuff>(fooGlobal);

        //    var innerScope = new VsExportProvisionScope(id);

        //    var clideContainer = VsCompositionContainer.Create(
        //        new TypeCatalog(typeof(ClideStuff)),
        //        new VsExportProviderSettings(
        //            innerScope,
        //            VsExportProvidingPreference.Default,
        //            VsExportSharingPolicy.ShareEverything,
        //            VsContainerHostingPolicy.DoNotAllowReuse));

        //    var outerScope = new VsExportProvisionOuterScope(id, innerScope);

        //    var vsixContainer = VsCompositionContainer.Create(
        //        new TypeCatalog(typeof(AdornmentStuff)),
        //        new VsExportProviderSettings(
        //            outerScope,
        //            VsExportProvidingPreference.Default,
        //            VsExportSharingPolicy.ShareEverything,
        //            VsContainerHostingPolicy.DoNotAllowReuse));

        //    var exportingContainer = VsCompositionContainer.Create(
        //        vsixContainer,
        //        new VsExportProviderSettings(
        //            VsExportProvidingPreference.Default,
        //            VsExportSharingPolicy.ShareEverything,
        //            VsContainerHostingPolicy.DoNotAllowReuse));

        //    try
        //    {
        //        var c1 = clideContainer.GetExportedValue<IClideStuff>();
        //        Assert.IsNotNull(c1);
        //    }
        //    catch (ImportCardinalityMismatchException)
        //    {
        //        Assert.Fail("Did not find IClideStuff on inner clide container.");
        //    }

        //    try
        //    {
        //        var a1 = vsixContainer.GetExportedValue<IAdornmentStuff>();
        //        Assert.IsNotNull(a1);
        //    }
        //    catch (ImportCardinalityMismatchException)
        //    {
        //        Assert.Fail("Did not find IAdornmentStuff on outer vsix extension container.");
        //    }

        //    try
        //    {
        //        var ga1 = globalContainer.GetExportedValue<IAdornmentStuff>();
        //        Assert.IsNotNull(ga1);
        //    }
        //    catch (ImportCardinalityMismatchException)
        //    {
        //        Assert.Fail("Did not find IAdornmentStuff on global VS container.");
        //    }

        //    try
        //    {
        //        globalContainer.GetExportedValue<IClideStuff>();
        //        Assert.Fail("Should not find IClideStuff on global container.");
        //    }
        //    catch (ImportCardinalityMismatchException)
        //    {
        //    }
        //}
    }

    public interface IClideStuff { }
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    [Export(typeof(IClideStuff))]
    public class ClideStuff : IClideStuff { }
}

namespace FooPackage
{
    public interface IAdornmentStuff
    {
        IClideStuff Clide { get; }
    }

    [Export(typeof(IAdornmentStuff))]
    public class AdornmentStuff : IAdornmentStuff
    {
        [Import]
        public IClideStuff Clide { get; set; }
    }
}