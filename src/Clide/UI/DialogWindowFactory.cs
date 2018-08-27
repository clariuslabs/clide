using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Windows;
using System.Windows.Interop;
using System.ComponentModel.Composition;
using Merq;
using Microsoft.VisualStudio.Threading;

namespace Clide
{

    /// <summary>
    /// Implements dialog creation in Visual Studio.
    /// </summary>
    [Export(typeof(IDialogWindowFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class DialogWindowFactory : IDialogWindowFactory
    {
        readonly Lazy<IVsUIShell> uiShell;
        readonly JoinableTaskFactory jtf;

        [ImportingConstructor]
        public DialogWindowFactory(
            [Import(ContractNames.Interop.IVsUIShell)] Lazy<IVsUIShell> uiShell,
            JoinableTaskContext context)
        {
            this.uiShell = uiShell;
            jtf = context.Factory;
        }

        public TView CreateDialog<TView>() where TView : IDialogWindow, new()
        {
            return jtf.Run(async () =>
            {
                await jtf.SwitchToMainThreadAsync();

                return CreateDialogImpl<TView>();
            });
        }

        TView CreateDialogImpl<TView>() where TView : IDialogWindow, new()
        {
            var dialog = new TView();
            if (dialog is Window dialogWindow)
            {
                jtf.Run(async () =>
                {
                    await jtf.SwitchToMainThreadAsync();
                    ErrorHandler.ThrowOnFailure(uiShell.Value.GetDialogOwnerHwnd(out var owner));
                    new WindowInteropHelper(dialogWindow).Owner = owner;
                    dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    dialogWindow.ShowInTaskbar = false;
                });
            }

            return dialog;
        }
    }
}