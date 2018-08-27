using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Clide.Components.Interop
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class VsServicesExports
    {
        JoinableTaskFactory jtf;
        JoinableLazy<IVsSolution> vsSolution;
        JoinableLazy<IVsStatusbar> vsStatusBar;
        JoinableLazy<IVsOutputWindow> outputWindow;
        JoinableLazy<IVsShell> vsShell;
        JoinableLazy<IVsUIShell> vsUIShell;

        [ImportingConstructor]
        public VsServicesExports(
            [Import(typeof(SVsServiceProvider))] IServiceProvider services,
            JoinableTaskContext context)
        {
            jtf = context.Factory;
            vsSolution = new JoinableLazy<IVsSolution>(() => services.GetService<SVsSolution, IVsSolution>(), jtf, true);
            vsStatusBar = new JoinableLazy<IVsStatusbar>(() => services.GetService<SVsStatusbar, IVsStatusbar>(), jtf, true);
            outputWindow = new JoinableLazy<IVsOutputWindow>(() => services.GetService<SVsOutputWindow, IVsOutputWindow>(), jtf, true);
            vsShell = new JoinableLazy<IVsShell>(() => services.GetService<SVsShell, IVsShell>(), jtf, true);
            vsUIShell = new JoinableLazy<IVsUIShell>(() => services.GetService<SVsUIShell, IVsUIShell>(), jtf, true);
        }

        [Export(ContractNames.Interop.VsSolution)]
        IVsSolution VsSolution => jtf.Run(async () => await vsSolution.GetValueAsync());

        [Export(ContractNames.Interop.VsStatusBar)]
        IVsStatusbar VsStatusBar => jtf.Run(async () => await vsStatusBar.GetValueAsync());

        [Export(ContractNames.Interop.VsOutputWindow)]
        IVsOutputWindow VsOutputWindow => jtf.Run(async () => await outputWindow.GetValueAsync());

        [Export(ContractNames.Interop.IVsShell)]
        IVsShell VsShell => jtf.Run(async () => await vsShell.GetValueAsync());

        [Export(ContractNames.Interop.IVsUIShell)]
        IVsUIShell VsUIShell => jtf.Run(async () => await vsUIShell.GetValueAsync());
    }
}