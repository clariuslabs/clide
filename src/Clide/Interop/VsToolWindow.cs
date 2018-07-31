using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Interop
{
    internal class VsToolWindow
    {
        private IVsUIShell uiShell;
        private Guid toolWindowId;

        public VsToolWindow(IServiceProvider serviceProvider, Guid toolWindowId)
        {
            uiShell = serviceProvider.GetService<SVsUIShell, IVsUIShell>();
            this.toolWindowId = toolWindowId;
        }

        public bool IsVisible
        {
            get
            {
                IVsWindowFrame frame;
                ErrorHandler.ThrowOnFailure(uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref toolWindowId, out frame));
                return frame != null && frame.IsVisible() == 0;
            }
        }

        public void Show()
        {
            IVsWindowFrame frame;
            ErrorHandler.ThrowOnFailure(uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref toolWindowId, out frame));
            if (frame != null)
            {
                ErrorHandler.ThrowOnFailure(frame.Show());
            }
        }

        public void Close()
        {
            IVsWindowFrame frame;
            ErrorHandler.ThrowOnFailure(uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref toolWindowId, out frame));
            if (frame != null)
            {
                ErrorHandler.ThrowOnFailure(frame.Hide());
            }
        }
    }
}
