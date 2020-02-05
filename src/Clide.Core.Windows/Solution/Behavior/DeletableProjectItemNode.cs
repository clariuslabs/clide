using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;

namespace Clide
{
    class DeletableProjectItemNode : IDeletableNode
    {
        readonly Lazy<IVsHierarchyItem> hierarchyNode;

        public DeletableProjectItemNode(IFolderNode node)
        {
            hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
        }

        public DeletableProjectItemNode(IItemNode node)
        {
            hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
        }

        public void Delete()
        {
            hierarchyNode.Value.GetExtenderObject<ProjectItem>().Delete();
        }
    }
}
