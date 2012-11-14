using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows;

namespace Clide.Diagnostics
{
    using System.ComponentModel.Composition;
    using System.Windows;
    using Clide.Composition;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Provides an implementation to send messages to the user using a message box.
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IUserMessageService))]
    public class UserMessageService : IUserMessageService
    {
        private IVsUIShell shell;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessageService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        [ImportingConstructor]
        public UserMessageService([Import(VsContractNames.IVsUIShell)] IVsUIShell shell)
        {
            this.shell = shell;
        }

        /// <summary>
        /// Shows an error to the user.
        /// </summary>
        /// <param name="message">The message to show.</param>
        public void ShowError(string message)
        {
            System.Windows.MessageBox.Show(this.shell.GetMainWindow(), message, "Visual Studio", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Shows information to the user.
        /// </summary>
        public void ShowInformation(string message)
        {
            this.shell.EnableModeless(0);
            try
            {
                ThreadHelper.Generic.Invoke(() =>
                    System.Windows.MessageBox.Show(this.shell.GetMainWindow(), message, "Visual Studio", MessageBoxButton.OK, MessageBoxImage.Information));
            }
            finally
            {
                this.shell.EnableModeless(1);
            }
        }

        /// <summary>
        /// Shows a warning to the user.
        /// </summary>
        /// <param name="message">The message to show.</param>
        public void ShowWarning(string message)
        {
            this.shell.EnableModeless(0);
            try
            {
                ThreadHelper.Generic.Invoke(() =>
                    System.Windows.MessageBox.Show(this.shell.GetMainWindow(), message, "Visual Studio", MessageBoxButton.OK, MessageBoxImage.Warning));
            }
            finally
            {
                this.shell.EnableModeless(1);
            }
        }

        /// <summary>
        /// Shows a warning prompting the user.
        /// </summary>
        /// <param name="message">The message to show.</param>
        public bool PromptWarning(string message)
        {
            this.shell.EnableModeless(0);
            try
            {
                return ThreadHelper.Generic.Invoke(() =>
                    System.Windows.MessageBox.Show(this.shell.GetMainWindow(), message, "Visual Studio", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK);
            }
            finally
            {
                this.shell.EnableModeless(1);
            }
        }
    }
}