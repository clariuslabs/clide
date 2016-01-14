using System.Collections.Generic;
using System.ComponentModel.Composition;
using Merq;

namespace Clide
{
	[Export (typeof (ICommandBus))]
	[PartCreationPolicy (CreationPolicy.Shared)]
	class CommandBus : Merq.CommandBus
	{
		[ImportingConstructor]
		public CommandBus ([ImportMany] IEnumerable<ICommandHandler> handlers)
			: base(handlers)
		{
		}
	}
}