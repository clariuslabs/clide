namespace Clide.Composition
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Provides core services as MEF exports, with our own custom <see cref="ContractNames"/> to 
    /// avoid potential collisions in the future.
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class ExportedServices
    {
        private IServiceProvider serviceProvider;

        [ImportingConstructor]
        public ExportedServices([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Export(ContractNames.ICompositionService)]
        public ICompositionService CompositionService
        {
            get { return this.serviceProvider.GetService<SComponentModel, IComponentModel>().DefaultCompositionService; }
        }

        [Export(ContractNames.ExportProvider)]
        public ExportProvider ExportProvider
        {
            get { return this.serviceProvider.GetService<SComponentModel, IComponentModel>().DefaultExportProvider; }
        }
    }
}
