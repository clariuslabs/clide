using System;
using System.ComponentModel.Composition;
using Clide.Sdk;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    [Export(ContractNames.FallbackNodeFactory, typeof(ICustomSolutionExplorerNodeFactory))]
    public class FolderNodeFactory : ICustomSolutionExplorerNodeFactory
    {
        Lazy<ISolutionExplorerNodeFactory> childNodeFactory;
        IAdapterService adapter;
        JoinableLazy<IVsUIHierarchyWindow> solutionExplorer;

        [ImportingConstructor]
        public FolderNodeFactory(
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

            if (extenderObject.GetType().FullName == "Microsoft.VisualStudio.Project.Automation.OAFolderItem")
                return true;

            try
            {
                // Fails in F# projects.
                return projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item) => Supports(item) ?
            new FolderNode(item, childNodeFactory.Value, adapter, solutionExplorer) : null;
    }
}
