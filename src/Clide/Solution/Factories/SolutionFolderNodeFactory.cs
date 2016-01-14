using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	[Export (ContractNames.FallbackNodeFactory, typeof (ICustomSolutionExplorerNodeFactory))]
	public class SolutionFolderNodeFactory : ICustomSolutionExplorerNodeFactory
	{
		Lazy<ISolutionExplorerNodeFactory> childNodeFactory;
		IAdapterService adapter;
		Lazy<IVsUIHierarchyWindow> solutionExplorer;

		[ImportingConstructor]
		public SolutionFolderNodeFactory (
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
			var project = item.GetExtenderObject() as EnvDTE.Project;

			return project != null &&
				project.Object is EnvDTE80.SolutionFolder;
		}

        public virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item)
		{
			return Supports(item) ?
				new SolutionFolderNode(item, childNodeFactory.Value, adapter, solutionExplorer) : null;
		}
	}
}