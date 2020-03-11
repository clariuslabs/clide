using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using Moq;
using System.ComponentModel.Composition;

namespace Clide
{
    internal class TestContainer : CompositionContainer
    {
        public TestContainer(ComposablePartCatalog catalog, IServiceProvider services = null)
            : base(catalog)
        {
            var provider = new ServiceLocatorImpl(services ?? Mock.Of<IServiceProvider>(), new Lazy<ExportProvider>(() => this));
            this.ComposeExportedValue<IServiceLocator>(provider);
            this.ComposeExportedValue<IServiceProvider>(provider);
        }
    }
}
