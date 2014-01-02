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
    using Clide.CommonComposition;
    using Clide.Composition;
    using Clide.Diagnostics;
    using Clide.Events;
    using Clide.Properties;
    using EnvDTE;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Component(IsSingleton = true)]
    internal class DevEnvImpl : IDevEnv
    {
        private static readonly Guid OutputWindowId = new Guid("{66893206-0EF5-4A16-AA10-6EC6B6319F92}");

        private Lazy<IStatusBar> status;
        private IShellEvents shellEvents;
        private Lazy<IDialogWindowFactory> dialogFactory;
        private IEnumerable<Lazy<IToolWindow>> toolWindows;
        private Lazy<IUIThread> uiThread;
        private Lazy<IMessageBoxService> messageBox;
        private Lazy<IReferenceService> references;
        private TraceOutputWindowManager outputWindowManager;
        private Lazy<bool> isElevated;
        private IErrorsManager errorsManager;

        public DevEnvImpl(
            ClideSettings settings,
            IServiceLocator serviceLocator,
            IEnumerable<Lazy<IToolWindow>> toolWindows,
            Lazy<IDialogWindowFactory> dialogFactory,
            Lazy<IUIThread> uiThread,
            Lazy<IMessageBoxService> messageBox,
            IShellEvents shellEvents,
            Lazy<IReferenceService> references,
            IErrorsManager errorsManager)
        {
            this.ServiceLocator = serviceLocator;
            this.dialogFactory = dialogFactory;
            this.toolWindows = toolWindows;
            this.shellEvents = shellEvents;
            this.uiThread = uiThread;
            this.messageBox = messageBox;
            this.status = new Lazy<IStatusBar>(() => new StatusBar(this.ServiceLocator));
            this.references = references;
            this.errorsManager = errorsManager;

            TracingExtensions.ErrorsManager = this.errorsManager;

            this.outputWindowManager = new TraceOutputWindowManager(
                serviceLocator,
                shellEvents,
                uiThread,
                Tracer.Manager,
                OutputWindowId,
                Strings.DevEnv.OutputPaneTitle);

            this.isElevated = new Lazy<bool>(() =>
            {
                var shell = this.ServiceLocator.TryGetService<SVsShell, IVsShell3>();
                if (shell == null)
                    return false;

                bool elevated;
                shell.IsRunningElevated(out elevated);
                return elevated;
            });

            Tracer.Manager.SetTracingLevel(TracerManager.DefaultSourceName, settings.TracingLevel);
        }

        public bool IsInitialized { get { return this.shellEvents.IsInitialized; } }

        public bool IsElevated { get { return this.isElevated.Value; } }

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

        public IReferenceService ReferenceService
        {
            get { return this.references.Value; }
        }

        public IErrorsManager Errors
        {
            get { return this.errorsManager; }
        }

        public event EventHandler Initialized
        {
            add { this.shellEvents.Initialized += value; }
            remove { this.shellEvents.Initialized -= value; }
        }

        public void Exit(bool saveAll = true)
        {
            var dte = ServiceLocator.GetInstance<DTE>();

            if (saveAll)
                dte.ExecuteCommand("File.SaveAll");

            // Just to be clean on exit, wait for pending builds to cancel.
            // VS will exit anyway if we don't, but this is safer.
            while (dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateInProgress)
            {
                // Sometimes when in the middle of some long-running build, the cancel command 
                // may not kick in immediately. We re-issue it after a bit.
                dte.ExecuteCommand("Build.Cancel");
                System.Threading.Thread.Sleep(100);
            }

            dte.Quit();
        }

        public bool Restart(bool saveAll = true)
        {
            var dte = ServiceLocator.GetInstance<DTE>();

            if (saveAll)
                dte.ExecuteCommand("File.SaveAll");

            // Just to be clean on exit, wait for pending builds to cancel.
            while (dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateInProgress)
            {
                // Sometimes when in the middle of some long-running build, the cancel command 
                // may not kick in immediately. We re-issue it after a bit.
                dte.ExecuteCommand("Build.Cancel");
                System.Threading.Thread.Sleep(100);
            }

            var shell = ServiceLocator.GetService<SVsShell, IVsShell4>();
            var result = shell.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);

            return ErrorHandler.Succeeded(result);
        }
    }
}
