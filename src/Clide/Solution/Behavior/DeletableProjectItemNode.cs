using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;

namespace Clide
{
	class DeletableProjectItemNode : BehaviorNode<IProjectItemNode, ProjectItem>, IDeletableNode
	{
		readonly Lazy<IVsHierarchyItem> hierarchyNode;

		public DeletableProjectItemNode(IFolderNode node)
			: base(node)
		{
			hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
		}

		public DeletableProjectItemNode(IItemNode node)
			: base(node)
		{
			hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
		}

		protected override Lazy<IVsHierarchyItem> HierarchyNode => hierarchyNode;

		public void Delete()
		{
			Automation.Value.Delete();
		}
	}
}