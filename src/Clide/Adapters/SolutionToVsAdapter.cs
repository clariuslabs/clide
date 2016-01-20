using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	[Adapter]
	class SolutionToVsAdapter :
		IAdapter<SolutionExplorerNode, IVsHierarchyItem>,
		IAdapter<SolutionExplorerNode, IVsHierarchy>,
		IAdapter<SolutionNode, IVsSolution>,
		IAdapter<ProjectNode, IVsProject>
	{
		IVsHierarchyItem IAdapter<SolutionExplorerNode, IVsHierarchyItem>.Adapt (SolutionExplorerNode from) => from?.HierarchyNode;

		IVsHierarchy IAdapter<SolutionExplorerNode, IVsHierarchy>.Adapt (SolutionExplorerNode from) => from?.HierarchyNode.GetActualHierarchy ();

		IVsSolution IAdapter<SolutionNode, IVsSolution>.Adapt (SolutionNode from) => from.HierarchyNode.GetServiceProvider ().GetService<SVsSolution, IVsSolution> ();

		IVsProject IAdapter<ProjectNode, IVsProject>.Adapt (ProjectNode from) => from.HierarchyNode.GetActualHierarchy () as IVsProject;
	}
}