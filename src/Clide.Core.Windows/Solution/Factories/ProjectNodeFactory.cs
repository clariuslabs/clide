using System;
using System.ComponentModel.Composition;
using Clide.Sdk;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Clide
{
    [Export(ContractNames.FallbackNodeFactory, typeof(ICustomSolutionExplorerNodeFactory))]
    public class ProjectNodeFactory : ICustomSolutionExplorerNodeFactory
    {
        JoinableLazy<IVsSolution> solution;
        IVsHierarchyItemManager hierarchyManager;
        Lazy<ISolutionExplorerNodeFactory> childNodeFactory;
        IAdapterService adapter;
        JoinableLazy<IVsUIHierarchyWindow> solutionExplorer;
        JoinableLazy<IVsSolution> vsSolution;
        JoinableLazy<IVsBooleanSymbolExpressionEvaluator> expressionEvaluator;
        JoinableTaskFactory asyncManager;

        [ImportingConstructor]
        public ProjectNodeFactory(
            [Import(typeof(SVsServiceProvider))] IServiceProvider services,
            IVsHierarchyItemManager hierarchyManager,
            Lazy<ISolutionExplorerNodeFactory> childNodeFactory,
            IAdapterService adapter,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer,
            JoinableTaskContext jtc,
            JoinableLazy<IVsSolution> vsSolution,
            [Import(Clide.ContractNames.Interop.IVsBooleanSymbolExpressionEvaluator)] JoinableLazy<IVsBooleanSymbolExpressionEvaluator> expressionEvaluator)
        {
            solution = new JoinableLazy<IVsSolution>(() => services.GetService<SVsSolution, IVsSolution>(), jtc.Factory, true);
            this.hierarchyManager = hierarchyManager;
            this.childNodeFactory = childNodeFactory;
            this.adapter = adapter;
            this.solutionExplorer = solutionExplorer;
            this.vsSolution = vsSolution;
            this.expressionEvaluator = expressionEvaluator;
            this.asyncManager = jtc.Factory;
        }

        public virtual bool Supports(IVsHierarchyItem item) => Supports(item, out item);

        public virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item) => Supports(item, out item) ?
            new ProjectNode(item, childNodeFactory.Value, adapter, solutionExplorer, expressionEvaluator) : null;

        bool Supports(IVsHierarchyItem item, out IVsHierarchyItem actualItem)
        {
            actualItem = item;
            if (!item.HierarchyIdentity.IsRoot)
                return false;

            // We need the hierarchy fully loaded if it's not yet.
            if (!vsSolution.GetValue().GetProperty<bool>(__VSPROPID4.VSPROPID_IsSolutionFullyLoaded))
            {
                // EnsureProjectIsLoaded MUST be executed in the UI/Main thread
                // Otherwise (if the Supports method is being invoked from a worker thread) duplicate keys might be generated
                actualItem = asyncManager.Run(async () =>
                {
                    await asyncManager.SwitchToMainThreadAsync();

                    Guid guid;
                    if (ErrorHandler.Succeeded(item.GetActualHierarchy().GetGuidProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out guid)) &&
                        // For the solution root item itself, the GUID will be empty.
                        guid != Guid.Empty)
                    {
                        if (ErrorHandler.Succeeded(((IVsSolution4)solution.GetValue()).EnsureProjectIsLoaded(ref guid, (uint)__VSBSLFLAGS.VSBSLFLAGS_None)))
                            return hierarchyManager.GetHierarchyItem(item.GetActualHierarchy(), item.GetActualItemId());
                    }

                    return item;
                });
            }

            var hierarchy = actualItem.GetActualHierarchy();
            if (!(actualItem.GetActualHierarchy() is IVsProject) && !(hierarchy is FlavoredProjectBase))
                return false;

            // Finally, solution folders look like projects, but they are not.
            // We need to filter them out too.
            var extenderObject = actualItem.GetExtenderObject();
            var dteProject = extenderObject as EnvDTE.Project;

            if (extenderObject != null && extenderObject.GetType().FullName == "Microsoft.VisualStudio.Project.Automation.OAProject")
                return false;

            if (extenderObject != null && dteProject != null && (dteProject.Object is EnvDTE80.SolutionFolder))
                return false;

            return true;
        }
    }
}
