using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide.Events
{
	public interface IShellEvents : IGlobalEvents
	{
		bool IsInitialized { get; }

		/// <summary>
		/// Occurs when the shell has finished initializing.
		/// </summary>
		event EventHandler Initialized;
	}
}
