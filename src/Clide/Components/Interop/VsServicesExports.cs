using System;
using System.ComponentModel.Composition;
using Merq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Components.Interop
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class VsServicesExports
    {
        Lazy<IVsSolution> vsSolution;
        Lazy<IVsStatusbar> vsStatusBar;
        Lazy<IVsOutputWindow> outputWindow;
        Lazy<IVsShell> vsShell;
        Lazy<IVsUIShell> vsUIShell;

        [ImportingConstructor]
        public VsServicesExports(
            [Import(typeof(SVsServiceProvider))] IServiceProvider services,
            IAsyncManager async)
        {
            vsSolution = new Lazy<IVsSolution>(() => services.GetService<SVsSolution, IVsSolution>());
            vsStatusBar = new Lazy<IVsStatusbar>(() => services.GetService<SVsStatusbar, IVsStatusbar>());
            outputWindow = new Lazy<IVsOutputWindow>(() => services.GetService<SVsOutputWindow, IVsOutputWindow>());
            vsShell = new Lazy<IVsShell>(() => services.GetService<SVsShell, IVsShell>());
            vsUIShell = new Lazy<IVsUIShell>(() => services.GetService<SVsUIShell, IVsUIShell>());
        }

        [Export(ContractNames.Interop.VsSolution)]
        IVsSolution VsSolution => vsSolution.Value;

        [Export(ContractNames.Interop.VsStatusBar)]
        IVsStatusbar VsStatusBar => vsStatusBar.Value;

        [Export(ContractNames.Interop.VsOutputWindow)]
        IVsOutputWindow VsOutputWindow => outputWindow.Value;

        [Export(ContractNames.Interop.IVsShell)]
        IVsShell VsShell => vsShell.Value;

        [Export(ContractNames.Interop.IVsUIShell)]
        IVsUIShell VsUIShell => vsUIShell.Value;
    }
}