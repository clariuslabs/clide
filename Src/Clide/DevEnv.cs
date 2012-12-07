#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using Clide.Commands;
    using Microsoft.VisualStudio.Shell;

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
