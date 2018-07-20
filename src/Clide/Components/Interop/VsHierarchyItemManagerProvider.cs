using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace Clide.Components.Interop
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class VsHierarchyItemManagerProvider
    {
        readonly JoinableLazy<IVsHierarchyItemManager> hierarchyManager;

        [ImportingConstructor]
        public VsHierarchyItemManagerProvider([Import(typeof(SAsyncServiceProvider))] IAsyncServiceProvider services, JoinableTaskContext context)
        {
            hierarchyManager = new JoinableLazy<IVsHierarchyItemManager>(async () => {
                var componentModel = await services.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;

                return componentModel?.GetService<IVsHierarchyItemManager>();
            }, context.Factory, executeOnMainThread: true);
        }

        [Export(ContractNames.Interop.IVsHierarchyItemManager)]
        public IVsHierarchyItemManager HierarchyManager => hierarchyManager.GetValue();
    }
}
