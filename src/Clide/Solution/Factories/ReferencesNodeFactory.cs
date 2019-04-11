using System;
using System.Linq;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    [Export(ContractNames.FallbackNodeFactory, typeof(ICustomSolutionExplorerNodeFactory))]
    public class ReferencesNodeFactory : ICustomSolutionExplorerNodeFactory
    {
        Lazy<ISolutionExplorerNodeFactory> childNodeFactory;
        IAdapterService adapter;
        JoinableLazy<IVsUIHierarchyWindow> solutionExplorer;

        [ImportingConstructor]
        public ReferencesNodeFactory(
            Lazy<ISolutionExplorerNodeFactory> childNodeFactory,
            IAdapterService adapter,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer)
        {
            this.childNodeFactory = childNodeFactory;
            this.adapter = adapter;
            this.solutionExplorer = solutionExplorer;
        }

        public virtual bool Supports(IVsHierarchyItem item)
        {
            // For performance reasons we're first checking if the
            // extender object is null which it's expected for
            // the ReferencesNode.
            //
            // Then we check for the 1033 localized Text string or finally
            // the first child to be a VSLangProj.Reference
            return item.GetExtenderObject() == null &&
                (
                    item.Text == "References" ||
                    item.Children.FirstOrDefault()?.GetExtenderObject() is VSLangProj.Reference
                );
        }

        public virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item) => Supports(item) ?
            new ReferencesNode(item, childNodeFactory.Value, adapter, solutionExplorer) : null;
    }
}