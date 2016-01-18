using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Merq;

namespace Clide.Components
{
	[Export (typeof (IEventStream))]
	[PartCreationPolicy (CreationPolicy.Shared)]
	class EventStream : Merq.EventStream
	{
		[ImportingConstructor]
		public EventStream ([ImportMany(typeof(IObservable<>))] IEnumerable<object> observables)
			: base(observables)
		{
		}
	}
}