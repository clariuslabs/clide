using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Clide
{
    /// <summary>
    /// An aggregate factory that delegates the <see cref="Supports"/> and 
    /// <see cref="CreateNode"/> implementations to the first factory 
    /// received in the constructor that supports the given model node.
    /// </summary>
    [Export(typeof(ISolutionExplorerNodeFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SolutionExplorerNodeFactory : ISolutionExplorerNodeFactory
    {
        List<ICustomSolutionExplorerNodeFactory> customFactories;
        List<ICustomSolutionExplorerNodeFactory> defaultFactories;
        IAdapterService adapter;
        JoinableLazy<IVsUIHierarchyWindow> solutionExplorer;
        JoinableLazy<IVsHierarchyItemManager> hierarchyManager;
        JoinableTaskFactory asyncManager;

        [ImportingConstructor]
        public SolutionExplorerNodeFactory(
            [ImportMany(ContractNames.FallbackNodeFactory)] IEnumerable<ICustomSolutionExplorerNodeFactory> defaultFactories,
            [ImportMany] IEnumerable<ICustomSolutionExplorerNodeFactory> customFactories,
            IAdapterService adapter,
            JoinableTaskContext jtc,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer,
            JoinableLazy<IVsHierarchyItemManager> hierarchyManager)
        {
            this.defaultFactories = defaultFactories.ToList();
            this.customFactories = customFactories.ToList();
            this.adapter = adapter;
            this.solutionExplorer = solutionExplorer;
            this.hierarchyManager = hierarchyManager;
            this.asyncManager = jtc.Factory;
        }

        IVsHierarchyItemManager HierarchyManager => hierarchyManager.GetValue();

        public ISolutionExplorerNode CreateNode(IVsHierarchyItem item)
        {
            if (item == null)
                return null;

            var factory = customFactories.FirstOrDefault(f => f.Supports(item)) ??
                defaultFactories.FirstOrDefault(f => f.Supports(item));

            return factory == null ? new GenericNode(item, this, adapter, solutionExplorer) : factory.CreateNode(item);
        }

        public ISolutionExplorerNode CreateNode(IVsHierarchy hierarchy, uint itemId = VSConstants.VSITEMID_ROOT) =>
            CreateNode(asyncManager.Run(async () =>
            {
                // TryGetHierarchyItem won't trigger the creation of the HierachyItem
                // It just tries to get the item from the already generated items
                // So there is no need to switch to the Main thread yet
                if (!HierarchyManager.TryGetHierarchyItem(hierarchy, itemId, out var hierarchyItem))
                {
                    // If the item was not created yet we MUST switch to the Main thread
                    // to avoid generating duplicate items
                    await asyncManager.SwitchToMainThreadAsync();

                    // GetHierarchyItem might might trigger the creation of the item so it
                    // MUST be executed in the main thread
                    hierarchyItem = HierarchyManager.GetHierarchyItem(hierarchy, itemId);
                }

                return hierarchyItem;
            }));
    }
}