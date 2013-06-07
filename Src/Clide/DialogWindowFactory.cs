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
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Interop;
    using Clide.Composition;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Implements dialog creation in Visual Studio.
    /// </summary>
    [Component(typeof(IDialogWindowFactory))]
    internal class DialogWindowFactory : IDialogWindowFactory
    {
        private Lazy<IDevEnv> devEnv;
        private IVsUIShell uiShell;
        private IUIThread uiThread;

        public DialogWindowFactory(Lazy<IDevEnv> devEnv, IVsUIShell uiShell, IUIThread uiThread)
        {
            this.devEnv = devEnv;
            this.uiShell = uiShell;
            this.uiThread = uiThread;
        }

        public TView CreateDialog<TView>() where TView : IDialogWindow, new()
        {
            return uiThread.Invoke<TView>(CreateDialogImpl<TView>);
        }

        private TView CreateDialogImpl<TView>() where TView : IDialogWindow, new()
        {
            var dialog = new TView();
            var dialogWindow = dialog as Window;
            if (dialogWindow != null)
            {
                IntPtr owner;
                ErrorHandler.ThrowOnFailure(this.uiShell.GetDialogOwnerHwnd(out owner));
                new WindowInteropHelper(dialogWindow).Owner = owner;
                dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialogWindow.ShowInTaskbar = false;
                // This would not set the right owner.
                //dialogWindow.Owner = Application.Current.MainWindow;
            }

            return dialog;
        }
    }
}