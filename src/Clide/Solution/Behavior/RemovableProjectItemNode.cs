using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;

namespace Clide
{
	class RemovableProjectItemNode : BehaviorNode<IProjectItemNode, ProjectItem>, IRemovableNode
	{
		readonly Lazy<IVsHierarchyItem> hierarchyNode;

		public RemovableProjectItemNode(IFolderNode node)
			: base(node)
		{
			hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
		}

		public RemovableProjectItemNode(IItemNode node)
			: base(node)
		{
			hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
		}

		protected override Lazy<IVsHierarchyItem> HierarchyNode => hierarchyNode;

		public void Remove()
		{
			Automation.Value.Remove();
		}
	}
}