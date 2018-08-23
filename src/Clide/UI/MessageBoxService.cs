using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Merq;
using Microsoft.VisualStudio.Threading;

namespace Clide
{

    /// <summary>
    /// Default implementation of the <see cref="IMessageBoxService"/>.
    /// </summary>
    [Export(typeof(IMessageBoxService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class MessageBoxService : IMessageBoxService
    {
        static readonly ITracer tracer = Tracer.Get<MessageBoxService>();

        readonly Lazy<IVsUIShell> uiShell;
        readonly JoinableTaskFactory jtf;


        /// <summary>
        /// Default constructor for runtime behavior that can't be mocked.
        /// </summary>
        [ImportingConstructor]
        public MessageBoxService(
            [Import(ContractNames.Interop.IVsUIShell)] Lazy<IVsUIShell> uiShell,
            JoinableTaskContext context)
        {
            this.uiShell = uiShell;
            jtf = context.Factory;
        }

        public bool? Show(string message,
            string title = MessageBoxServiceDefaults.DefaultTitle,
            MessageBoxButton button = MessageBoxServiceDefaults.DefaultButton,
            MessageBoxImage icon = MessageBoxServiceDefaults.DefaultIcon,
            MessageBoxResult defaultResult = MessageBoxServiceDefaults.DefaultResult)
        {
            return jtf.Run(async () =>
            {
                await jtf.SwitchToMainThreadAsync();
                return uiShell.Value.ShowMessageBox(message, title, button, icon, defaultResult);
            });  
        }

        public MessageBoxResult Prompt(string message,
            string title = MessageBoxServiceDefaults.DefaultTitle,
            MessageBoxButton button = MessageBoxServiceDefaults.DefaultButton,
            MessageBoxImage icon = MessageBoxImage.Question,
            MessageBoxResult defaultResult = MessageBoxServiceDefaults.DefaultResult)
        {
            return jtf.Run(async () =>
            {
                await jtf.SwitchToMainThreadAsync();
                return uiShell.Value.Prompt(message, title, button, icon, defaultResult);
            });
        }
    }
}