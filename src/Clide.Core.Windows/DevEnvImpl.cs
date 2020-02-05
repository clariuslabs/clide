using System;
using System.ComponentModel.Composition;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Clide
{

    [Export(typeof(IDevEnv))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class DevEnvImpl : IDevEnv
    {
        readonly JoinableTaskFactory jtf;

        readonly Lazy<IServiceLocator> serviceLocator;
        readonly Lazy<IStatusBar> status;
        readonly Lazy<IDialogWindowFactory> dialogFactory;
        readonly Lazy<IMessageBoxService> messageBox;
        readonly JoinableLazy<bool> isElevated;
        readonly Lazy<IErrorsManager> errorsManager;
        readonly Lazy<IOutputWindowManager> outputWindow;
        readonly JoinableLazy<DevEnvInfo> devEnvInfo;

        [ImportingConstructor]
        public DevEnvImpl(
            Lazy<IServiceLocator> serviceLocator,
            Lazy<IDialogWindowFactory> dialogFactory,
            Lazy<IMessageBoxService> messageBox,
            Lazy<IErrorsManager> errorsManager,
            Lazy<IOutputWindowManager> outputWindow,
            Lazy<IStatusBar> status,
            JoinableLazy<DevEnvInfo> devEnvInfo,
            JoinableTaskContext context)
        {
            jtf = context.Factory;
            this.serviceLocator = serviceLocator;
            this.dialogFactory = dialogFactory;
            this.messageBox = messageBox;
            this.status = status;
            this.errorsManager = errorsManager;
            this.outputWindow = outputWindow;
            this.devEnvInfo = devEnvInfo;

            TracingExtensions.ErrorsManager = this.errorsManager.Value;

            isElevated = JoinableLazy.Create(() =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var shell = ServiceLocator.TryGetService<SVsShell, IVsShell3>();
                if (shell == null)
                    return false;

                shell.IsRunningElevated(out var elevated);
                return elevated;
            }, jtf, executeOnMainThread: true);
        }

        public bool IsElevated => isElevated.GetValue();

        public DevEnvInfo Info => devEnvInfo.GetValue();

        public IDialogWindowFactory DialogWindowFactory => dialogFactory.Value;

        public IErrorsManager Errors => errorsManager.Value;

        public IMessageBoxService MessageBoxService => messageBox.Value;

        public IOutputWindowManager OutputWindow => outputWindow.Value;

        public IServiceLocator ServiceLocator => serviceLocator.Value;

        public IStatusBar StatusBar => status.Value;

        public void Exit(bool saveAll = true)
        {
            jtf.Run(async () =>
            {
                await jtf.SwitchToMainThreadAsync();
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
                    await System.Threading.Tasks.Task.Delay(100);
                }

                dte.Quit();
            });
        }

        public bool Restart(bool saveAll = true)
        {
            return jtf.Run(async () =>
            {
                await jtf.SwitchToMainThreadAsync();
                var dte = ServiceLocator.GetService<DTE>();

                if (saveAll)
                    dte.ExecuteCommand("File.SaveAll");

                // Just to be clean on exit, wait for pending builds to cancel.
                while (dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateInProgress)
                {
                    // Sometimes when in the middle of some long-running build, the cancel command 
                    // may not kick in immediately. We re-issue it after a bit.
                    dte.ExecuteCommand("Build.Cancel");
                    await System.Threading.Tasks.Task.Delay(100);
                }

                var shell = ServiceLocator.GetService<SVsShell, IVsShell4>();
                var result = shell.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);

                return ErrorHandler.Succeeded(result);
            });
        }
    }
}
