using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;

namespace Clide
{
	[Adapter]
	class SolutionToHierarchy : IAdapter<SolutionExplorerNode, IVsHierarchyItem>
	{
		public IVsHierarchyItem Adapt (SolutionExplorerNode from)
		{
			Guard.NotNull ("from", from);

			return from.HierarchyNode;
		}
	}
}