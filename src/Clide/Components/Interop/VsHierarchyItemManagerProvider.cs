using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace Clide.Components.Interop
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class VsHierarchyItemManagerProvider
    {
        readonly AsyncLazy<IVsHierarchyItemManager> hierarchyManager;

        [ImportingConstructor]
        public VsHierarchyItemManagerProvider([Import(typeof(SVsServiceProvider))] IServiceProvider services)
        {
            hierarchyManager = new AsyncLazy<IVsHierarchyItemManager>(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                return services.GetService<SComponentModel, IComponentModel>().GetService<IVsHierarchyItemManager>();
            }, ThreadHelper.JoinableTaskFactory);
        }

        [Export(ContractNames.Interop.IVsHierarchyItemManager)]
        public IVsHierarchyItemManager HierarchyManager => ThreadHelper.JoinableTaskFactory.Run(async () => await hierarchyManager.GetValueAsync());
    }
}
