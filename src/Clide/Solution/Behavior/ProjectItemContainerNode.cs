using Clide.Properties;
using Microsoft.VisualStudio.Shell;
using System;
using System.Linq;

namespace Clide
{
	abstract class ProjectItemContainerNode<TNode, TAutomation> : BehaviorNode<TNode, TAutomation>, IProjectItemContainerNode
		where TNode : ISolutionExplorerNode
	{
		public ProjectItemContainerNode(TNode node, Lazy<ISolutionExplorerNodeFactory> nodeFactory)
			: base(node)
		{
			this.NodeFactory = nodeFactory;
		}

		public Lazy<ISolutionExplorerNodeFactory> NodeFactory { get; private set; }

		public abstract IItemNode AddItem(string path);

		public abstract IFolderNode CreateFolder(string name);

		protected virtual T GetProjectItemNode<T>(string name)
			where T : IProjectItemNode
		{
			var item = GetChildHierarchyItem(name);
			if (item == null)
				throw new InvalidOperationException(Strings.ProjectItemContainerNode.ItemNotFound(name, Node.Name));

			return (T)NodeFactory.Value.CreateNode(item);
		}

		IVsHierarchyItem GetChildHierarchyItem(string name)
		{
			return HierarchyNode.Value.Children.FirstOrDefault(child =>
				child.GetProperty<string>(VsHierarchyPropID.Name) == name);
		}
	}
}