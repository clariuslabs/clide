using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Merq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Components.Interop
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class VsSolution
	{
		Lazy<IVsSolution> vsSolution;

		[ImportingConstructor]
		public VsSolution([Import(typeof(SVsServiceProvider))] IServiceProvider services, IAsyncManager async)
		{
			vsSolution = new Lazy<IVsSolution>(() => async.Run(async () =>
			{
				await async.SwitchToMainThread();
				return services.GetService<SVsSolution, IVsSolution>();
			}));
		}

		[Export(ContractNames.Interop.VsSolution)]
		public IVsSolution Solution => vsSolution.Value;
	}
}