using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Merq;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;

namespace Clide.Components.Interop
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class MenuCommandServiceProvider
	{
		Lazy<IMenuCommandService> vsShell;

		[ImportingConstructor]
		public MenuCommandServiceProvider([Import(typeof(SVsServiceProvider))] IServiceProvider services, IAsyncManager async)
		{
			vsShell = new Lazy<IMenuCommandService>(() => async.Run(async () =>
			{
				await async.SwitchToMainThread();
				return services.GetService<IMenuCommandService>();
			}));
		}

		[Export(ContractNames.Interop.IVsShell)]
		public IMenuCommandService HierarchyManager => vsShell.Value;
	}
}