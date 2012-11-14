using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using System;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Clide.Events;
using Clide.Commands;

namespace Clide
{
	[Export(typeof(IDevEnv))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class DevEnv : IDevEnv, IShellEvents
	{
		private Lazy<IStatusBar> status;
		private IShellEvents shellEvents;
		private Lazy<ICommandManager> commands;
		private Lazy<IDialogWindowFactory> dialogFactory;
		private IEnumerable<Lazy<IToolWindow>> toolWindows;
		private Lazy<IUIThread> uiThread;

		[ImportingConstructor]
		public DevEnv(
			[Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
			[ImportMany] IEnumerable<Lazy<IToolWindow>> toolWindows,
			Lazy<ICommandManager> commandManager,
			Lazy<IDialogWindowFactory> dialogFactory,
			Lazy<IUIThread> uiThread,
			IShellEvents shellEvents)
		{
			this.ServiceProvider = serviceProvider;
			this.commands = commandManager;
			this.dialogFactory = dialogFactory;
			this.toolWindows = toolWindows;
			this.shellEvents = shellEvents;
			this.uiThread = uiThread;
			this.status = new Lazy<IStatusBar>(() => new StatusBar(this.ServiceProvider));
		}

		internal IServiceProvider ServiceProvider { get; set; }

		public IStatusBar Status
		{
			get { return this.status.Value; }
		}

		public IUIThread UIThread
		{
			get { return this.uiThread.Value; }
		}

		public ICommandManager Commands
		{
			get { return this.commands.Value; }
		}

		public IDialogWindowFactory Dialogs
		{
			get { return this.dialogFactory.Value; }
		}

		public IEnumerable<IToolWindow> ToolWindows
		{
			get { return this.toolWindows.Select(lazy => lazy.Value); }
		}

		public bool IsInitialized { get { return this.shellEvents.IsInitialized; } }

		public event EventHandler Initialized
		{
			add { this.shellEvents.Initialized += value; }
			remove { this.shellEvents.Initialized -= value; }
		}
	}
}
