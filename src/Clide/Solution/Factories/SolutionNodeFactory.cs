
using System;
using System.ComponentModel.Composition;
using Clide.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    [Export(ContractNames.FallbackNodeFactory, typeof(ICustomSolutionExplorerNodeFactory))]
    public class SolutionNodeFactory : ICustomSolutionExplorerNodeFactory
    {
        IServiceProvider services;
        Lazy<ISolutionExplorerNodeFactory> nodeFactory;
        IAdapterService adapter;
        IVsSolutionSelection selection;
        JoinableLazy<IVsUIHierarchyWindow> solutionExplorer;

        [ImportingConstructor]
        public SolutionNodeFactory(
            [Import(typeof(SVsServiceProvider))] IServiceProvider services,
            Lazy<ISolutionExplorerNodeFactory> nodeFactory,
            IAdapterService adapter,
            IVsSolutionSelection selection,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer)
        {
            this.services = services;
            this.nodeFactory = nodeFactory;
            this.adapter = adapter;
            this.selection = selection;
            this.solutionExplorer = solutionExplorer;
        }

        public virtual bool Supports(IVsHierarchyItem item) => item.HierarchyIdentity.Hierarchy is IVsSolution;

        public virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item) => Supports(item) ?
            new SolutionNode(services, item, nodeFactory.Value, adapter, selection, solutionExplorer) : null;
    }
}