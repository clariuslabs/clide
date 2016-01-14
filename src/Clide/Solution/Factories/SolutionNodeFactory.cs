using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Clide.Interop;

namespace Clide
{
	[Export (ContractNames.FallbackNodeFactory, typeof (ICustomSolutionExplorerNodeFactory))]
	public class SolutionNodeFactory : ICustomSolutionExplorerNodeFactory
	{
		Lazy<ISolutionExplorerNodeFactory> nodeFactory;
		IAdapterService adapter;
		IVsSolutionSelection selection;
		Lazy<IVsUIHierarchyWindow> solutionExplorer;

		[ImportingConstructor]
		public SolutionNodeFactory(
			Lazy<ISolutionExplorerNodeFactory> nodeFactory,
            IAdapterService adapter,
			IVsSolutionSelection selection,
			[Import (ContractNames.Interop.SolutionExplorerWindow)] Lazy<IVsUIHierarchyWindow> solutionExplorer)
		{
			this.nodeFactory = nodeFactory;
			this.adapter = adapter;
			this.selection = selection;
			this.solutionExplorer = solutionExplorer;
		}

        public virtual bool Supports(IVsHierarchyItem item)
		{
			return item.HierarchyIdentity.Hierarchy is IVsSolution;
		}

        public virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item)
		{
			return Supports(item) ?
				new SolutionNode(item, nodeFactory.Value, adapter, selection, solutionExplorer) : null;
		}
	}
}