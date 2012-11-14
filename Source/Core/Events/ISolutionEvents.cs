using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide.Events
{
	public interface ISolutionEvents : IGlobalEvents
	{
		event EventHandler SolutionOpened;
		event EventHandler SolutionClosing;
		event EventHandler SolutionClosed;
	}
}
