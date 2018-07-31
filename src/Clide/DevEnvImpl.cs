using EnvDTE;
using Merq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
namespace Clide
{

    [Export(typeof(IDevEnv))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class DevEnvImpl : IDevEnv
    {
        readonly Lazy<IServiceLocator> serviceLocator;
        readonly Lazy<IStatusBar> status;
        readonly Lazy<IDialogWindowFactory> dialogFactory;
        readonly Lazy<IMessageBoxService> messageBox;
        readonly Lazy<bool> isElevated;
        readonly Lazy<IErrorsManager> errorsManager;
        readonly Lazy<IOutputWindowManager> outputWindow;

        [ImportingConstructor]
        public DevEnvImpl(
            Lazy<IServiceLocator> serviceLocator,
            Lazy<IDialogWindowFactory> dialogFactory,
            Lazy<IAsyncManager> asyncManager,
            Lazy<IMessageBoxService> messageBox,
            Lazy<IErrorsManager> errorsManager,
            Lazy<IOutputWindowManager> outputWindow,
            Lazy<IStatusBar> status)
        {
            this.serviceLocator = serviceLocator;
            this.dialogFactory = dialogFactory;
            this.messageBox = messageBox;
            this.status = status;
            this.errorsManager = errorsManager;
            this.outputWindow = outputWindow;

            TracingExtensions.ErrorsManager = this.errorsManager.Value;

            isElevated = new Lazy<bool>(() =>
            {
                var shell = this.ServiceLocator.TryGetService<SVsShell, IVsShell3>();
                if (shell == null)
                    return false;

                bool elevated;
                shell.IsRunningElevated(out elevated);
                return elevated;
            });
        }

        public bool IsElevated => isElevated.Value;

        public IDialogWindowFactory DialogWindowFactory => dialogFactory.Value;

        public IErrorsManager Errors => errorsManager.Value;

        public IMessageBoxService MessageBoxService => messageBox.Value;

        public IOutputWindowManager OutputWindow => outputWindow.Value;

        public IServiceLocator ServiceLocator => serviceLocator.Value;

        public IStatusBar StatusBar => status.Value;

        public void Exit(bool saveAll = true)
        {
            var dte = ServiceLocator.GetService<DTE>();

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
            var dte = ServiceLocator.GetService<DTE>();

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