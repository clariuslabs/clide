using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reactive.Disposables;
using Merq;
using Xunit;

namespace Clide.Components
{
	public class EventStreamSpec
	{
		[Fact]
		public void when_container_exports_multiple_observers_then_gets_for_derived_classes()
		{
			var container = new TestContainer(new TypeCatalog(typeof(EventStream), typeof(ObservableBase), typeof(ObservableDerived)));
			var stream = container.GetExportedValue<IEventStream>();

			var events = new List<BaseEvent>();
			stream.Of<BaseEvent>().Subscribe(e => events.Add(e));

			Assert.Equal(2, events.Count);
		}

		[Export]
		public class ImportingExports
		{
			[ImportingConstructor]
			public ImportingExports(IServiceProvider locator)
			{
			}
		}

		[PartCreationPolicy(CreationPolicy.Shared)]
		[Export(typeof(IObservable<BaseEvent>))]
		public class ObservableBase : IObservable<BaseEvent>
		{
			public IDisposable Subscribe(IObserver<BaseEvent> observer)
			{
				observer.OnNext(new BaseEvent());
				return Disposable.Empty;
			}
		}

		[PartCreationPolicy(CreationPolicy.Shared)]
		[Export(typeof(IObservable<BaseEvent>))]
		[Export(typeof(IObservable<DerivedEvent>))]
		public class ObservableDerived : IObservable<DerivedEvent>
		{
			public IDisposable Subscribe(IObserver<DerivedEvent> observer)
			{
				observer.OnNext(new DerivedEvent());
				return Disposable.Empty;
			}
		}

		public class BaseEvent { }
		public class DerivedEvent : BaseEvent { }
	}
}
