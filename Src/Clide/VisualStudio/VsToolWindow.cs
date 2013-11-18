#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.VisualStudio
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;

    internal class VsToolWindow
    {
        private IVsUIShell uiShell;
        private Guid toolWindowId;

        public VsToolWindow(IServiceProvider serviceProvider, Guid toolWindowId)
        {
            this.uiShell = serviceProvider.GetService<SVsUIShell, IVsUIShell>();
            this.toolWindowId = toolWindowId;
        }

        public bool IsVisible
        {
            get
            {
                IVsWindowFrame frame;
                ErrorHandler.ThrowOnFailure(this.uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref toolWindowId, out frame));
                return frame != null && frame.IsVisible() == 0;
            }
        }

        public void Show()
        {
            IVsWindowFrame frame;
            ErrorHandler.ThrowOnFailure(this.uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref toolWindowId, out frame));
            if (frame != null)
            {
                ErrorHandler.ThrowOnFailure(frame.Show());
            }
        }

        public void Close()
        {
            IVsWindowFrame frame;
            ErrorHandler.ThrowOnFailure(this.uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref toolWindowId, out frame));
            if (frame != null)
            {
                ErrorHandler.ThrowOnFailure(frame.Hide());
            }
        }
    }
}
