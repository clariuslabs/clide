using System;
using System.ComponentModel.Composition;
using Clide.Sdk;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    [Export(ContractNames.FallbackNodeFactory, typeof(ICustomSolutionExplorerNodeFactory))]
    public class ItemNodeFactory : ICustomSolutionExplorerNodeFactory
    {
        Lazy<ISolutionExplorerNodeFactory> childNodeFactory;
        IAdapterService adapter;
        JoinableLazy<IVsUIHierarchyWindow> solutionExplorer;

        [ImportingConstructor]
        public ItemNodeFactory(
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
            var extenderObject = item.GetExtenderObject();
            var projectItem = extenderObject as EnvDTE.ProjectItem;

            if (extenderObject == null || projectItem == null)
                return false;

            try
            {
                // Fails in F# projects.
                return projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item) => Supports(item) ?
            new ItemNode(item, childNodeFactory.Value, adapter, solutionExplorer) : null;
    }
}