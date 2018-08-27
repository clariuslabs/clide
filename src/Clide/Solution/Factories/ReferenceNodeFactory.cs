using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    [Export(ContractNames.FallbackNodeFactory, typeof(ICustomSolutionExplorerNodeFactory))]
    public class ReferenceNodeFactory : ICustomSolutionExplorerNodeFactory
    {
        Lazy<ISolutionExplorerNodeFactory> childNodeFactory;
        IAdapterService adapter;
        Lazy<IVsUIHierarchyWindow> solutionExplorer;

        [ImportingConstructor]
        public ReferenceNodeFactory(
            Lazy<ISolutionExplorerNodeFactory> childNodeFactory,
            IAdapterService adapter,
            [Import(ContractNames.Interop.SolutionExplorerWindow)] Lazy<IVsUIHierarchyWindow> solutionExplorer)
        {
            this.childNodeFactory = childNodeFactory;
            this.adapter = adapter;
            this.solutionExplorer = solutionExplorer;
        }

        public virtual bool Supports(IVsHierarchyItem item) => item.GetExtenderObject() is VSLangProj.Reference;

        public virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item) => Supports(item) ?
            new ReferenceNode(item, childNodeFactory.Value, adapter, solutionExplorer) : null;
    }
}