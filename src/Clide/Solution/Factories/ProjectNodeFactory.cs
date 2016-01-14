using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	[Export (ContractNames.FallbackNodeFactory, typeof (ICustomSolutionExplorerNodeFactory))]
	public class ProjectNodeFactory : ICustomSolutionExplorerNodeFactory
	{
		Lazy<IVsSolution> solution;
		IVsHierarchyItemManager hierarchyManager;
		Lazy<ISolutionExplorerNodeFactory> childNodeFactory;
		IAdapterService adapter;
		Lazy<IVsUIHierarchyWindow> solutionExplorer;

		[ImportingConstructor]
		public ProjectNodeFactory (
			[Import(typeof(SVsServiceProvider))] IServiceProvider services, 
			IVsHierarchyItemManager hierarchyManager,
			Lazy<ISolutionExplorerNodeFactory> childNodeFactory,
			IAdapterService adapter,
			[Import (ContractNames.Interop.SolutionExplorerWindow)] Lazy<IVsUIHierarchyWindow> solutionExplorer)
		{
			solution = new Lazy<IVsSolution> (() => services.GetService<SVsSolution, IVsSolution> ());
			this.hierarchyManager = hierarchyManager;
            this.childNodeFactory = childNodeFactory;
			this.adapter = adapter;
			this.solutionExplorer = solutionExplorer;
		}

        public virtual bool Supports(IVsHierarchyItem item)
		{
			return Supports (item, out item);
		}

        public virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item)
		{
			return Supports(item, out item) ?
				new ProjectNode(item, childNodeFactory.Value, adapter, solutionExplorer) : null;
		}

		bool Supports (IVsHierarchyItem item, out IVsHierarchyItem actualItem)
		{
			actualItem = item;
			if (!item.HierarchyIdentity.IsRoot)
				return false;

			// We need the hierarchy fully loaded if it's not yet.
			if (!item.GetProperty<bool> (__VSPROPID4.VSPROPID_IsSolutionFullyLoaded)) {
				Guid guid;
				if (ErrorHandler.Succeeded (item.GetActualHierarchy ().GetGuidProperty ((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out guid)) &&
					// For the solution root item itself, the GUID will be empty.
					guid != Guid.Empty) {
					if (ErrorHandler.Succeeded (((IVsSolution4)solution.Value).EnsureProjectIsLoaded (ref guid, (uint)__VSBSLFLAGS.VSBSLFLAGS_None)))
						actualItem = hierarchyManager.GetHierarchyItem (item.GetActualHierarchy (), item.GetActualItemId ());
				}
			}

			if (!(actualItem.GetActualHierarchy () is IVsProject))
				return false;

			// Finally, solution folders look like projects, but they are not.
			// We need to filter them out too.
			var extenderObject = actualItem.GetExtenderObject();
			var dteProject = extenderObject as EnvDTE.Project;

			if (extenderObject != null && extenderObject.GetType ().FullName == "Microsoft.VisualStudio.Project.Automation.OAProject")
				return false;

			if (extenderObject != null && dteProject != null && (dteProject.Object is EnvDTE80.SolutionFolder))
				return false;

			return true;
		}
	}
}