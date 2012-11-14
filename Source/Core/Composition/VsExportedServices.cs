using EnvDTE;
namespace Clide.Composition
{
    using System;
    using System.ComponentModel.Composition;
    using EnvDTE;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Provides core VS services as MEF exports, with our own custom <see cref="VsContractNames"/> to 
    /// avoid potential collisions in the future.
    /// </summary>
    /// <devdoc>
    /// It's a best practice in VS-MEF world to not re-expose services you don't own without a custom 
    /// contract name. This allows the service owner to provide the service as a MEF export in the 
    /// future, without risking to break your package.
    /// </devdoc>
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class VsExportedServices
    {
        private IServiceProvider serviceProvider;

        [ImportingConstructor]
        public VsExportedServices([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Export(VsContractNames.IComponentModel)]
        public IComponentModel ComponentModel
        {
            get { return this.serviceProvider.GetService<SComponentModel, IComponentModel>(); }
        }

        [Export(VsContractNames.IVsUIShell)]
        public IVsUIShell UIShell
        {
            get { return this.serviceProvider.GetService<SVsUIShell, IVsUIShell>(); }
        }

        [Export(VsContractNames.IVsShell)]
        public IVsShell VsShell
        {
            get { return this.serviceProvider.GetService<SVsShell, IVsShell>(); }
        }

        [Export(VsContractNames.DTE)]
        public DTE DTE
        {
            get { return this.serviceProvider.GetService<SDTE, DTE>(); }
        }
    }
}
