using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Merq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Components.Interop
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class VsStatusBar
	{
		Lazy<IVsStatusbar> vsStatusBar;

		[ImportingConstructor]
		public VsStatusBar([Import(typeof(SVsServiceProvider))] IServiceProvider services, IAsyncManager async)
		{
			vsStatusBar = new Lazy<IVsStatusbar>(() => async.Run(async () =>
			{
				await async.SwitchToMainThread();
				return services.GetService<SVsStatusbar, IVsStatusbar>();
			}));
		}

		[Export(ContractNames.Interop.VsStatusBar)]
		public IVsStatusbar StatusBar => vsStatusBar.Value;
	}
}