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