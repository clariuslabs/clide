using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
namespace Clide
{

    [Export(typeof(IStatusBar))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class StatusBar : IStatusBar
    {
        readonly JoinableLazy<IVsStatusbar> vsStatusBar;

        [ImportingConstructor]
        public StatusBar(JoinableLazy<IVsStatusbar> vsStatusBar)
        {
            this.vsStatusBar = vsStatusBar;
        }

        public void Clear() => vsStatusBar.GetValue().Clear();

        public void ShowMessage(string message)
        {
            int frozen;

            vsStatusBar.GetValue().IsFrozen(out frozen);

            if (frozen == 0)
                vsStatusBar.GetValue().SetText(message);
        }

        public void ShowProgress(string message, int complete, int total)
        {
            int frozen;

            vsStatusBar.GetValue().IsFrozen(out frozen);

            if (frozen == 0)
            {
                uint cookie = 0;

                if (complete != total)
                    vsStatusBar.GetValue().Progress(ref cookie, 1, message, (uint)complete, (uint)total);
                else
                    vsStatusBar.GetValue().Progress(ref cookie, 0, string.Empty, (uint)complete, (uint)total);
            }
        }
    }
}
