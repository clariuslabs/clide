using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Merq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Clide
{
	/// <summary>
	/// We switch the implementation of the async manager depending 
	/// on the Visual Studio version. This allows us to use it 
	/// right now on VS2012/VS2013 even before Microsoft ships the 
	/// v12 nuget package to the gallery.
	/// </summary>
	[PartCreationPolicy (CreationPolicy.Shared)]
	class AsyncManagerProvider
	{
		IServiceProvider serviceProvider;
		Lazy<IAsyncManager> manager;

		[ImportingConstructor]
		public AsyncManagerProvider ([Import (typeof (SVsServiceProvider))] IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
			manager = new Lazy<IAsyncManager> (() => CreateAsyncManager ());
		}

		[Export]
		public IAsyncManager AsyncManager => manager.Value;

		IAsyncManager CreateAsyncManager ()
		{
			// VS2012 case
			var context = serviceProvider.TryGetService<SVsJoinableTaskContext, JoinableTaskContext> ();
			if (context != null)
				return new AsyncManager (context);

			// VS2013+ case
			var schedulerService = serviceProvider.TryGetService<SVsTaskSchedulerService, IVsTaskSchedulerService2>();
			if (schedulerService != null)
				return new AsyncManager ((JoinableTaskContext)schedulerService.GetAsyncTaskContext ());

			// Just let the default constructor initialize a task context. 
			// Should never get here.
			return new AsyncManager ();
		}

		/// <summary>
		/// The legacy VS2012 type serving as the service guid for acquiring an instance of JoinableTaskContext.
		/// </summary>
		[Guid ("8767A7D4-ECFC-4627-8FC0-A1685E3B0493")]
		interface SVsJoinableTaskContext
		{
		}
	}
}
