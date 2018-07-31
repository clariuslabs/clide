using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
namespace Clide
{

    [Export(typeof(IStatusBar))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class StatusBar : IStatusBar
    {
        readonly Lazy<IVsStatusbar> vsStatusBar;

        [ImportingConstructor]
        public StatusBar([Import(ContractNames.Interop.VsStatusBar)] Lazy<IVsStatusbar> vsStatusBar)
        {
            this.vsStatusBar = vsStatusBar;
        }

        public void Clear() => vsStatusBar.Value.Clear();

        public void ShowMessage(string message)
        {
            int frozen;

            vsStatusBar.Value.IsFrozen(out frozen);

            if (frozen == 0)
                vsStatusBar.Value.SetText(message);
        }

        public void ShowProgress(string message, int complete, int total)
        {
            int frozen;

            vsStatusBar.Value.IsFrozen(out frozen);

            if (frozen == 0)
            {
                uint cookie = 0;

                if (complete != total)
                    vsStatusBar.Value.Progress(ref cookie, 1, message, (uint)complete, (uint)total);
                else
                    vsStatusBar.Value.Progress(ref cookie, 0, string.Empty, (uint)complete, (uint)total);
            }
        }
    }
}