using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Merq;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Components.Interop
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class VsOutputWindowProvider
	{
		Lazy<IVsOutputWindow> outputWindow;

		[ImportingConstructor]
		public VsOutputWindowProvider([Import(typeof(SVsServiceProvider))] IServiceProvider services, IAsyncManager async)
		{
			outputWindow = new Lazy<IVsOutputWindow>(() => async.Run(async () =>
			{
				await async.SwitchToMainThread();
				return services.GetService<SVsOutputWindow, IVsOutputWindow>();
			}));
		}

		[Export(ContractNames.Interop.VsOutputWindow)]
		public IVsOutputWindow VsOutputWindow => outputWindow.Value;
	}
}