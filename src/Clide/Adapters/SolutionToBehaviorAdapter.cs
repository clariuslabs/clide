using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;

namespace Clide
{
    [Adapter]
    class SolutionToBehaviorAdapter :
        IAdapter<FolderNode, IProjectItemContainerNode>,
        IAdapter<ProjectNode, IProjectItemContainerNode>,
        IAdapter<ItemNode, IDeletableNode>,
        IAdapter<FolderNode, IDeletableNode>,
        IAdapter<ItemNode, IRemovableNode>,
        IAdapter<FolderNode, IRemovableNode>,
        IAdapter<ProjectNode, IReferenceContainerNode>,
        IAdapter<ReferencesNode, IReferenceContainerNode>,
        IAdapter<SolutionNode, IProjectContainerNode>
    {
        readonly Lazy<ISolutionExplorerNodeFactory> nodeFactory;

        [ImportingConstructor]
        public SolutionToBehaviorAdapter(Lazy<ISolutionExplorerNodeFactory> nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        IProjectItemContainerNode IAdapter<ProjectNode, IProjectItemContainerNode>.Adapt(ProjectNode from) =>
            new ProjectItemContainerNode(from, nodeFactory);

        IProjectItemContainerNode IAdapter<FolderNode, IProjectItemContainerNode>.Adapt(FolderNode from) =>
            new ProjectItemContainerNode(from, nodeFactory);

        IDeletableNode IAdapter<ItemNode, IDeletableNode>.Adapt(ItemNode from) =>
            new DeletableProjectItemNode(from);

        IDeletableNode IAdapter<FolderNode, IDeletableNode>.Adapt(FolderNode from) =>
            new DeletableProjectItemNode(from);

        IRemovableNode IAdapter<ItemNode, IRemovableNode>.Adapt(ItemNode from) =>
            new RemovableProjectItemNode(from);

        IRemovableNode IAdapter<FolderNode, IRemovableNode>.Adapt(FolderNode from) =>
            new RemovableProjectItemNode(from);

        IReferenceContainerNode IAdapter<ProjectNode, IReferenceContainerNode>.Adapt(ProjectNode from) =>
            new ReferenceContainerNode(from);

        IReferenceContainerNode IAdapter<ReferencesNode, IReferenceContainerNode>.Adapt(ReferencesNode from) =>
            new ReferenceContainerNode(from);

        IProjectContainerNode IAdapter<SolutionNode, IProjectContainerNode>.Adapt(SolutionNode from) =>
            new ProjectContainerNode(from);
    }
}