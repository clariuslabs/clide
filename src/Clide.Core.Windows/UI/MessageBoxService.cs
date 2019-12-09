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

        readonly JoinableLazy<IVsUIShell> uiShell;
        readonly JoinableTaskFactory jtf;


        /// <summary>
        /// Default constructor for runtime behavior that can't be mocked.
        /// </summary>
        [ImportingConstructor]
        public MessageBoxService(
            JoinableLazy<IVsUIShell> uiShell,
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
                return uiShell.GetValue().ShowMessageBox(message, title, button, icon, defaultResult);
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
                return uiShell.GetValue().Prompt(message, title, button, icon, defaultResult);
            });
        }
    }
}