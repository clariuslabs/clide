using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;

namespace Clide
{
    [Adapter]
    internal class VsToSolutionAdapter :
        IAdapter<IVsHierarchy, IProjectNode>,
        IAdapter<FlavoredProject, IProjectNode>
    {
        readonly Lazy<ISolutionExplorerNodeFactory> nodeFactory;
        readonly JoinableLazy<IVsHierarchyItemManager> hierarchyItemManager;
        readonly JoinableTaskFactory asyncManager;

        [ImportingConstructor]
        public VsToSolutionAdapter(
            JoinableTaskContext jtc,
            Lazy<ISolutionExplorerNodeFactory> nodeFactory,
            JoinableLazy<IVsHierarchyItemManager> hierarchyItemManager)
        {
            asyncManager = jtc.Factory;
            this.nodeFactory = nodeFactory;
            this.hierarchyItemManager = hierarchyItemManager;
        }

        public IProjectNode Adapt(IVsHierarchy from) =>
            from is FlavoredProjectBase && from.TryGetInnerHierarchy(out var innerHierarchy) ?
                Adapt(new FlavoredProject(from, innerHierarchy)) :
                nodeFactory
                    .Value
                    .CreateNode(GetHierarchyItem(from))
                    as IProjectNode;

        public IProjectNode Adapt(FlavoredProject from) =>
            (nodeFactory
                .Value
                .CreateNode(GetHierarchyItem(from.InnerHierarchy))
                as ProjectNode).WithFlavorHierarchy(from.Hierarchy);

        IVsHierarchyItem GetHierarchyItem(IVsHierarchy hierarchy) =>
            asyncManager.Run(async () =>
            {
                await asyncManager.SwitchToMainThreadAsync();

                return (await hierarchyItemManager.GetValueAsync()).GetHierarchyItem(hierarchy, VSConstants.VSITEMID_ROOT);
            });
    }
}
