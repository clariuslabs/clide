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

namespace Clide.IntegrationPackage
{
	[Guid(Constants.PackageGuid)]
	// NOTE: autoload required for the tools|options page registration.
	[ProvideAutoLoad(UIContextGuids.NoSolution)]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class ShellPackage : Package, ISamplePackage
	{
		[Import]
		private IDevEnv devEnv;

		protected override void Initialize()
		{
			base.Initialize();

			var composition = ((IComponentModel)this.GetService(typeof(SComponentModel)));
			composition.DefaultCompositionService.SatisfyImportsOnce(this);

			devEnv.Commands.AddCommands(this);
            devEnv.Commands.AddFilters(this);
        }
	}
}