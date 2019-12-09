
using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Clide.Components.Interop
{
    class VsUIHierarchyWindowProvider
    {
        IServiceProvider serviceProvider;

        [Export(ContractNames.Interop.SolutionExplorerWindow)]
        [Export(typeof(JoinableLazy<IVsUIHierarchyWindow>))]
        JoinableLazy<IVsUIHierarchyWindow> solutionExplorer;

        [ImportingConstructor]
        public VsUIHierarchyWindowProvider(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            JoinableTaskContext jtc)
        {
            this.serviceProvider = serviceProvider;
            solutionExplorer = new JoinableLazy<IVsUIHierarchyWindow>(() => GetHierarchyWindow(EnvDTE.Constants.vsWindowKindSolutionExplorer), jtc.Factory, true);
        }

        [Export(ContractNames.Interop.SolutionExplorerWindow)]
        public IVsUIHierarchyWindow SolutionExplorer => solutionExplorer.GetValue();


        IVsUIHierarchyWindow GetHierarchyWindow(string windowKind)
        {
            var uiShell = serviceProvider.GetService<SVsUIShell, IVsUIShell>();
            object pvar = null;
            IVsWindowFrame frame;
            var persistenceSlot = new Guid(windowKind);
            if (ErrorHandler.Succeeded(uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref persistenceSlot, out frame)) && frame != null)
                ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out pvar));

            return (IVsUIHierarchyWindow)pvar;
        }
    }
}