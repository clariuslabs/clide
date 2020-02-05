using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;

namespace Clide
{
    class RemovableProjectItemNode : IRemovableNode
    {
        readonly Lazy<IVsHierarchyItem> hierarchyNode;

        public RemovableProjectItemNode(IFolderNode node)
        {
            hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
        }

        public RemovableProjectItemNode(IItemNode node)
        {
            hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
        }

        public void Remove()
        {
            hierarchyNode.Value.GetExtenderObject<ProjectItem>().Remove();
        }
    }
}
