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
    using Clide.Diagnostics;
    using System.Diagnostics;
    using Clide.Properties;
    using System.ComponentModel.Composition.Hosting;
    using Clide.Events;
    using Microsoft.Practices.ServiceLocation;
    using Clide.Composition;

    [Component(typeof(IDevEnv))]
	internal class DevEnvImpl : IDevEnv, IShellEvents
	{
        private static readonly Guid OutputWindowId = new Guid("{66893206-0EF5-4A16-AA10-6EC6B6319F92}");

        private Lazy<IStatusBar> status;
		private IShellEvents shellEvents;
		private Lazy<IDialogWindowFactory> dialogFactory;
		private IEnumerable<Lazy<IToolWindow>> toolWindows;
		private Lazy<IUIThread> uiThread;
        private Lazy<IMessageBoxService> messageBox;
        private TraceOutputWindowManager outputWindowManager;

		public DevEnvImpl(
			IServiceLocator serviceLocator,
			IEnumerable<Lazy<IToolWindow>> toolWindows,
			Lazy<IDialogWindowFactory> dialogFactory,
			Lazy<IUIThread> uiThread,
            Lazy<IMessageBoxService> messageBox,
			IShellEvents shellEvents)
		{
            this.ServiceLocator = serviceLocator;
			this.dialogFactory = dialogFactory;
			this.toolWindows = toolWindows;
			this.shellEvents = shellEvents;
			this.uiThread = uiThread;
            this.messageBox = messageBox;
            this.status = new Lazy<IStatusBar>(() => new StatusBar(this.ServiceLocator));

            this.outputWindowManager = new TraceOutputWindowManager(
                serviceLocator,
                shellEvents,
                uiThread,
                Tracer.Manager,
                OutputWindowId,
                Strings.DevEnv.OutputPaneTitle);
		}

        public IServiceLocator ServiceLocator { get; private set; }

		public IStatusBar StatusBar
		{
			get { return this.status.Value; }
		}
         
		public IUIThread UIThread
		{
			get { return this.uiThread.Value; }
		}

		public IDialogWindowFactory DialogWindowFactory
		{
			get { return this.dialogFactory.Value; }
		}

        public IMessageBoxService MessageBoxService
        {
            get { return this.messageBox.Value; }
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
