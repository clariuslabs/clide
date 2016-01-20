using System.Threading;
using System.Threading.Tasks;
using Merq;
using Microsoft.VisualStudio.ComponentModelHost;
using Xunit;
using Xunit.Abstractions;

namespace Clide
{
	public class AsyncManagerSpec
	{
		ITestOutputHelper output;

		public AsyncManagerSpec (ITestOutputHelper output)
		{
			this.output = output;
		}

		[VsixFact]
		public void when_switching_contexts_then_succeeds ()
		{
			var manager = GlobalServices.GetService<SComponentModel, IComponentModel>().GetService<IAsyncManager>();

			manager.Run (async () => {
				await manager.SwitchToMainThread ();
				var mainThreadId = Thread.CurrentThread.ManagedThreadId;

				var backgroundId = 0;
				var foregroundId = mainThreadId;

				await manager.SwitchToBackground ();

				backgroundId = Thread.CurrentThread.ManagedThreadId;

				Assert.NotEqual (backgroundId, foregroundId);

				await manager.SwitchToMainThread ();

				foregroundId = Thread.CurrentThread.ManagedThreadId;
				Assert.Equal (mainThreadId, foregroundId);

				await manager.RunAsync (async () => {
					Assert.Equal (foregroundId, Thread.CurrentThread.ManagedThreadId);
					output.WriteLine ("RunAsync on {0} thread", (Thread.CurrentThread.ManagedThreadId == foregroundId) ? "main" : "background");
					await Task.Yield ();
				});

				var message = await manager.RunAsync(async () => {
					Assert.Equal (foregroundId, Thread.CurrentThread.ManagedThreadId);
					return await Task.FromResult(string.Format ("RunAsync<string> on {0} thread", (Thread.CurrentThread.ManagedThreadId == foregroundId) ? "main" : "background"));
				});

				output.WriteLine (message);

				await manager.SwitchToBackground ();

				Assert.NotEqual (foregroundId, Thread.CurrentThread.ManagedThreadId);

				await manager.RunAsync (async () => {
					Assert.NotEqual (foregroundId, Thread.CurrentThread.ManagedThreadId);
					output.WriteLine ("RunAsync on {0} thread", (Thread.CurrentThread.ManagedThreadId == foregroundId) ? "main" : "background");
					await Task.Yield ();
				});

				message = await manager.RunAsync (async () => {
					Assert.NotEqual (foregroundId, Thread.CurrentThread.ManagedThreadId);
					return await Task.FromResult (string.Format ("RunAsync<string> on {0} thread", (Thread.CurrentThread.ManagedThreadId == foregroundId) ? "main" : "background"));
				});

				output.WriteLine (message);
			});
		}
	}
}
