using System;
using System.Linq;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Clide.Sdk;

namespace Clide
{
    [Export(ContractNames.FallbackNodeFactory, typeof(ICustomSolutionExplorerNodeFactory))]
    public class ReferencesNodeFactory : ICustomSolutionExplorerNodeFactory
    {
        Lazy<ISolutionExplorerNodeFactory> childNodeFactory;
        IAdapterService adapter;
        JoinableLazy<IVsUIHierarchyWindow> solutionExplorer;
        JoinableTaskFactory asyncManager;

        [ImportingConstructor]
        public ReferencesNodeFactory(
            Lazy<ISolutionExplorerNodeFactory> childNodeFactory,
            IAdapterService adapter,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer,
            JoinableTaskContext jtc)
        {
            this.childNodeFactory = childNodeFactory;
            this.adapter = adapter;
            this.solutionExplorer = solutionExplorer;
            this.asyncManager = jtc.Factory;
        }

        public virtual bool Supports(IVsHierarchyItem item)
        {
            var result = false;

            // For performance reasons we're first checking if the
            // extender object is null which it's expected for
            // the ReferencesNode
            if (item.GetExtenderObject() == null && item.Parent?.GetExtenderObject() is EnvDTE.Project)
            {
                // Then we first check for the localized Text string 
                result = item.Text == "References" || item.Text == "Referencias";

                if (!result)
                {
                    // Or first if any child is an instance of VSLangProj.Reference
                    // It's important to call .Children to avoid ending up with duplicate
                    // element when Children are still not created
                    // And also we need to use Any because the first item might be
                    // a PackageReference which does not provide an extender object
                    result = asyncManager.Run(async () =>
                    {
                        await asyncManager.SwitchToMainThreadAsync();

                        return item.Children.Any(x => x.GetExtenderObject() is VSLangProj.Reference);
                    });
                }
            }

            return result;
        }

        public virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item) => Supports(item) ?
            new ReferencesNode(item, childNodeFactory.Value, adapter, solutionExplorer) : null;
    }
}
