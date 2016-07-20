using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Merq;

namespace Clide.Components
{
	[Export(typeof(IEventStream))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	class EventStream : Merq.EventStream
	{
		static readonly Type genericObservable = typeof(IObservable<>);
		// We cache the retrieved observables by type since retrieving them 
		// isn't particularly cheap, since we need to construct the IObservable<TEvent> 
		// via reflection.
		ConcurrentDictionary<Type, IEnumerable<object>> observables = new ConcurrentDictionary<Type, IEnumerable<object>>();
		IServiceLocator services;

		[ImportingConstructor]
		public EventStream(IServiceLocator services)
		{
			this.services = services;
		}

		protected override IEnumerable<IObservable<TEvent>> GetObservables<TEvent>()
		{
			return (IEnumerable<IObservable<TEvent>>)observables.GetOrAdd(typeof(TEvent), eventType =>
			{
				var producers = new List<IObservable<TEvent>>();
				var concreteObservable = genericObservable.MakeGenericType(eventType);
				foreach (var observable in services.GetExports(concreteObservable))
				{
					producers.Add((IObservable<TEvent>)observable);
				}
				return producers;
			});
		}
	}
}