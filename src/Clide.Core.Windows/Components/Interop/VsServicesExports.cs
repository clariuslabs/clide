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

        [Export(ContractNames.Interop.VsSolution)]
        [Export(typeof(JoinableLazy<IVsSolution>))]
        JoinableLazy<IVsSolution> vsSolution;

        [Export(ContractNames.Interop.VsStatusBar)]
        [Export(typeof(JoinableLazy<IVsStatusbar>))]
        JoinableLazy<IVsStatusbar> vsStatusBar;

        [Export(ContractNames.Interop.VsOutputWindow)]
        [Export(typeof(JoinableLazy<IVsOutputWindow>))]
        JoinableLazy<IVsOutputWindow> outputWindow;

        [Export(ContractNames.Interop.IVsShell)]
        [Export(typeof(JoinableLazy<IVsShell>))]
        JoinableLazy<IVsShell> vsShell;

        [Export(ContractNames.Interop.IVsUIShell)]
        [Export(typeof(JoinableLazy<IVsUIShell>))]
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
        IVsSolution VsSolution => vsSolution.GetValue();

        [Export(ContractNames.Interop.VsStatusBar)]
        IVsStatusbar VsStatusBar => vsStatusBar.GetValue();

        [Export(ContractNames.Interop.VsOutputWindow)]
        IVsOutputWindow VsOutputWindow => outputWindow.GetValue();

        [Export(ContractNames.Interop.IVsShell)]
        IVsShell VsShell => vsShell.GetValue();

        [Export(ContractNames.Interop.IVsUIShell)]
        IVsUIShell VsUIShell => vsUIShell.GetValue();
    }
}