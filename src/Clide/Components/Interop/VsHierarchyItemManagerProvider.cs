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
        readonly JoinableTaskContext context;
		readonly AsyncLazy<IVsHierarchyItemManager> hierarchyManager;

		[ImportingConstructor]
		public VsHierarchyItemManagerProvider ([Import (typeof (SAsyncServiceProvider))] IAsyncServiceProvider services, JoinableTaskContext context)
		{
            this.context = context;
			hierarchyManager = new AsyncLazy<IVsHierarchyItemManager> (async () => {
                await context.Factory.SwitchToMainThreadAsync();

                var componentModel = await services.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;

                return componentModel.GetService<IVsHierarchyItemManager>();
            }, context.Factory);
		}

		[Export(ContractNames.Interop.IVsHierarchyItemManager)]
		public IVsHierarchyItemManager HierarchyManager => context.Factory.Run (async () => await hierarchyManager.GetValueAsync ());
    }
}
