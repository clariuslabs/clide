namespace IntegrationPackage
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Linq;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using Clide;

    [Guid(Constants.PackageGuid)]
	[ProvideAutoLoad(UIContextGuids.NoSolution)]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	public class ShellPackage : Package, IShellPackage
	{
        private IHost<ShellPackage, IShellPackage> host;
        
        public ShellPackage()
        {
            this.host = HostFactory.CreateHost<ShellPackage, IShellPackage>(ServiceProvider.GlobalProvider, "Clide.IntegrationTests");
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.host.Initialize(this);
        }

        [Import]
        public IDevEnv DevEnv { get; set; }

        public ICompositionService Composition
        {
            get { return this.host.Composition; }
        }
    }
}
