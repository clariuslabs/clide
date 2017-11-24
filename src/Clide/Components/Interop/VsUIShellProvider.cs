using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Merq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Components.Interop
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class VsUIShellProvider
	{
		Lazy<IVsUIShell> vsUIShell;

		[ImportingConstructor]
		public VsUIShellProvider([Import(typeof(SVsServiceProvider))] IServiceProvider services, IAsyncManager async)
		{
			vsUIShell = new Lazy<IVsUIShell>(() => async.Run(async () =>
			{
				await async.SwitchToMainThread();
				return services.GetService<SVsUIShell, IVsUIShell>();
			}));
		}

		[Export(ContractNames.Interop.IVsUIShell)]
		public IVsUIShell VsUIShell => vsUIShell.Value;
	}
}