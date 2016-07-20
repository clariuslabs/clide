using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Merq;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Components.Interop
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class VsShellProvider
	{
		Lazy<IVsShell> vsShell;

		[ImportingConstructor]
		public VsShellProvider([Import(typeof(SVsServiceProvider))] IServiceProvider services, IAsyncManager async)
		{
			vsShell = new Lazy<IVsShell>(() => async.Run(async () => {
				await async.SwitchToMainThread();
				return services.GetService<SVsShell, IVsShell>();
			}));
		}

		[Export(ContractNames.Interop.IVsShell)]
		public IVsShell HierarchyManager => vsShell.Value;
	}
}
