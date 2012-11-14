using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using System.ComponentModel.Composition.Hosting;
using System.Threading.Tasks;

namespace Clide
{
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
