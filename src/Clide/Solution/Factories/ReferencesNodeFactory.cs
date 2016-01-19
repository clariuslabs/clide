using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	[Export (ContractNames.FallbackNodeFactory, typeof (ICustomSolutionExplorerNodeFactory))]
	public class ReferencesNodeFactory : ICustomSolutionExplorerNodeFactory
	{
		Lazy<ISolutionExplorerNodeFactory> childNodeFactory;
		IAdapterService adapter;
		Lazy<IVsUIHierarchyWindow> solutionExplorer;

		[ImportingConstructor]
		public ReferencesNodeFactory (
			Lazy<ISolutionExplorerNodeFactory> childNodeFactory,
			IAdapterService adapter,
			[Import (ContractNames.Interop.SolutionExplorerWindow)] Lazy<IVsUIHierarchyWindow> solutionExplorer)
		{
			this.childNodeFactory = childNodeFactory;
			this.adapter = adapter;
			this.solutionExplorer = solutionExplorer;
		}

        public virtual bool Supports(IVsHierarchyItem item)
		{
			// TODO: why wouldn't something like this work? 
			// 
			//	 ((VSProject)((Project)hierarchyNode.GetExtenderObject ()).Object).References);

			// \o/: heuristics, no extender object and "References" display name :S
			var extObj = item.GetExtenderObject();
			// TODO: see if we get the VSProject from VSLangProj and compare to the References property there.
            return extObj == null && item.Text == "References";
		}

		public virtual ISolutionExplorerNode CreateNode (IVsHierarchyItem item) => Supports (item) ?
			new ReferencesNode (item, childNodeFactory.Value, adapter, solutionExplorer) : null;
	}
}