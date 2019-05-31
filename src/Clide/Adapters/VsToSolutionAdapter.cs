using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    [Adapter]
    internal class VsToSolutionAdapter :
        IAdapter<IVsHierarchy, IProjectNode>,
        IAdapter<FlavoredProject, IProjectNode>
    {
        readonly Lazy<ISolutionExplorerNodeFactory> nodeFactory;

        [ImportingConstructor]
        public VsToSolutionAdapter(Lazy<ISolutionExplorerNodeFactory> nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        public IProjectNode Adapt(IVsHierarchy from) =>
            from is FlavoredProjectBase && from.TryGetInnerHierarchy(out var innerHierarchy) ?
                Adapt(new FlavoredProject(from, innerHierarchy)) :
                nodeFactory
                    .Value
                    .CreateNode(from)
                    as IProjectNode;

        public IProjectNode Adapt(FlavoredProject from) =>
            (nodeFactory
                .Value
                .CreateNode(from.InnerHierarchy)
                as ProjectNode).WithFlavorHierarchy(from.Hierarchy);
    }
}