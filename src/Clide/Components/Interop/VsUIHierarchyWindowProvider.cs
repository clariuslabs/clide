
using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Components.Interop
{
	class VsUIHierarchyWindowProvider
	{
		IServiceProvider serviceProvider;
		Lazy<IVsUIHierarchyWindow> solutionExplorer;

		[ImportingConstructor]
		public VsUIHierarchyWindowProvider ([Import (typeof (SVsServiceProvider))] IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
			solutionExplorer = new Lazy<IVsUIHierarchyWindow> (() => GetHierarchyWindow (EnvDTE.Constants.vsWindowKindSolutionExplorer));
		}

		[Export (ContractNames.Interop.SolutionExplorerWindow)]
		public IVsUIHierarchyWindow SolutionExplorer { get { return solutionExplorer.Value; } }


		IVsUIHierarchyWindow GetHierarchyWindow (string windowKind)
		{
			var uiShell = serviceProvider.GetService<SVsUIShell, IVsUIShell>();
			object pvar = null;
			IVsWindowFrame frame;
			var persistenceSlot = new Guid(windowKind);
			if (ErrorHandler.Succeeded (uiShell.FindToolWindow ((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref persistenceSlot, out frame)) && frame != null)
				ErrorHandler.ThrowOnFailure (frame.GetProperty ((int)__VSFPROPID.VSFPROPID_DocView, out pvar));

			return (IVsUIHierarchyWindow)pvar;
		}
	}
}