using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Microsoft.VisualStudio.Threading;
using System.Reactive.Disposables;

namespace Clide
{
	public class ReactiveSpec
	{
		[Fact]
		public async Task when_subscribing_observable_then_can_async_wait_on_value()
		{
			var e = new object();
			var initializedEvent = new AsyncManualResetEvent();

			var observable = Observable.Create<object>(async o =>
			{
				await initializedEvent.WaitAsync();
				o.OnNext(e);
				o.OnCompleted();

				return Disposable.Empty;
			});

			object obj1 = null;
			bool completed1 = false;

			var s1 = observable.Subscribe(o => obj1 = o, () => completed1 = true);

			Assert.False(completed1);

			await initializedEvent.SetAsync();

			SpinWait.SpinUntil(() => completed1, 200);

			Assert.Same(e, obj1);
			Assert.True(completed1);

			object obj2 = null;
			bool completed2 = false;

			var s2 = observable.Subscribe(o => obj2 = o, () => completed2 = true);

			Assert.True(completed2);
			Assert.Same(e, obj2);
		}
	}
}
