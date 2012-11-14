using System.ComponentModel.Design;
using System.Collections.Generic;
using System;
using Clide.Events;
using Clide.Commands;

namespace Clide
{
	public interface IDevEnv : IShellEvents, IFluentInterface
	{
		ICommandManager Commands { get; }
		IDialogWindowFactory Dialogs { get; }
		IStatusBar Status { get; }
		IEnumerable<IToolWindow> ToolWindows { get; }
		IUIThread UIThread { get; }
	}
}