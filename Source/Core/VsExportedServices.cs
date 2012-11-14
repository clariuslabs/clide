using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using System.Runtime.InteropServices;

namespace Clide
{
	/// <summary>
	/// Provides core VS services as MEF exports, with our own custom <see cref="VsContractNames"/> to 
	/// avoid potential collisions in the future.
	/// </summary>
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
