using Clide.Properties;
using Clide.Sdk;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;

namespace Clide
{
    class ProjectItemContainerNode : IProjectItemContainerNode
    {
        ISolutionExplorerNode node;
        readonly Lazy<ISolutionExplorerNodeFactory> nodeFactory;
        readonly Lazy<ProjectItems> projectItems;
        readonly Lazy<IVsHierarchyItem> hierarchyNode;

        public ProjectItemContainerNode(IProjectNode node, Lazy<ISolutionExplorerNodeFactory> nodeFactory)
            : this(
                  node,
                  nodeFactory,
                  new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem()),
                  new Lazy<ProjectItems>(() => node.AsVsHierarchyItem().GetExtenderObject<Project>().ProjectItems))
        { }

        public ProjectItemContainerNode(IFolderNode node, Lazy<ISolutionExplorerNodeFactory> nodeFactory)
            : this(
                  node,
                  nodeFactory,
                  new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem()),
                  new Lazy<ProjectItems>(() => node.AsVsHierarchyItem().GetExtenderObject<ProjectItem>().ProjectItems))
        { }

        ProjectItemContainerNode(
            ISolutionExplorerNode node,
            Lazy<ISolutionExplorerNodeFactory> nodeFactory,
            Lazy<IVsHierarchyItem> hierarchyNode,
            Lazy<ProjectItems> projectItems)
        {
            this.node = node;
            this.nodeFactory = nodeFactory;
            this.hierarchyNode = hierarchyNode;
            this.projectItems = projectItems;
        }

        public virtual IItemNode AddItem(string path)
        {
            projectItems.Value.AddFromFileCopy(path);

            return GetProjectItemNode<IItemNode>(Path.GetFileName(path));
        }

        public virtual IFolderNode CreateFolder(string name)
        {
            Guard.NotNullOrEmpty(nameof(name), name);

            projectItems.Value.AddFolder(name);

            return GetProjectItemNode<IFolderNode>(name);
        }

        protected virtual T GetProjectItemNode<T>(string name)
            where T : IProjectItemNode
        {
            var item = GetChildHierarchyItem(name);
            if (item == null)
                throw new InvalidOperationException(Strings.ProjectItemContainerNode.ItemNotFound(name, node.Name));

            return (T)nodeFactory.Value.CreateNode(item);
        }

        IVsHierarchyItem GetChildHierarchyItem(string name)
        {
            return hierarchyNode.Value.Children.FirstOrDefault(child =>
                child.GetProperty<string>(VsHierarchyPropID.Name) == name);
        }
    }
}