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
using Clide.Hosting;

namespace Clide.IntegrationPackage
{
	[Guid(Constants.PackageGuid)]
	[ProvideAutoLoad(UIContextGuids.NoSolution)]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	public class ShellPackage : HostingPackage
	{
        protected override string CatalogName { get { return "Clide.IntegrationTests"; } }

        [Export(Constants.PackageContract)]
        public IHostingPackage Package { get { return this.LoadedPackage.Value; } }
    }
}