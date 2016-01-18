using System;
using System.ComponentModel.Composition;
using Merq;

namespace Clide
{
	/// <summary>
	/// Attribute applied to command handlers to export them to the 
	/// composition container.
	/// </summary>
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
	public class CommandHandlerAttribute : ExportAttribute
	{
		/// <summary>
		/// Initializes the attribute.
		/// </summary>
		public CommandHandlerAttribute () 
			: base(typeof(ICommandHandler))
		{
		}
	}
}
