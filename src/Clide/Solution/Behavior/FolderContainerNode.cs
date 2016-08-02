using Clide.Properties;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;

namespace Clide
{
	class FolderContainerNode : ProjectItemContainerNode<IFolderNode, ProjectItem>
	{
		readonly Lazy<IVsHierarchyItem> hierarchyNode;

		public FolderContainerNode(IFolderNode node, Lazy<ISolutionExplorerNodeFactory> nodeFactory)
			: base(node, nodeFactory)
		{
			hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
		}
		protected override Lazy<IVsHierarchyItem> HierarchyNode => hierarchyNode;

		public override IItemNode AddItem(string path)
		{
			Automation.Value.ProjectItems.AddFromFileCopy(path);

			return GetProjectItemNode<IItemNode>(Path.GetFileName(path));
		}

		public override IFolderNode CreateFolder(string name)
		{
			Guard.NotNullOrEmpty(nameof(name), name);

			Automation.Value.ProjectItems.AddFolder(name);

			return GetProjectItemNode<IFolderNode>(name);
		}
	}
}
