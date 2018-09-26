using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Linq;
using Clide.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Clide
{
    [Export(typeof(ISolutionExplorer))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class SolutionExplorer : ISolutionExplorer
    {
        Lazy<VsToolWindow> toolWindow;
        IServiceProvider services;
        IVsHierarchyItemManager hierarchy;
        ISolutionExplorerNodeFactory factory;
        JoinableTaskFactory asyncManager;

        [ImportingConstructor]
        public SolutionExplorer(
            [Import(typeof(SVsServiceProvider))] IServiceProvider services,
            // Get the IVsHierarchyItemManager from our provider, so that it's a singleton 
            // and provided always from a UI thread. The default exported one from VS doesn't 
            // have the PartCreationPolicy.Shared attribute and is newed up in whatever context 
            // you're requesting it from.
            [Import(ContractNames.Interop.IVsHierarchyItemManager)] IVsHierarchyItemManager hierarchy,
            ISolutionExplorerNodeFactory factory,
            JoinableTaskContext jtc)
        {
            Guard.NotNull(nameof(services), services);
            Guard.NotNull(nameof(hierarchy), hierarchy);
            Guard.NotNull(nameof(factory), factory);

            this.services = services;
            this.hierarchy = hierarchy;
            this.factory = factory;
            this.asyncManager = jtc.Factory;
            toolWindow = new Lazy<VsToolWindow>(() => new VsToolWindow(services, StandardToolWindows.ProjectExplorer));
        }

        public Awaitable<ISolutionNode> Solution => Awaitable.Create(async () =>
        {
            await asyncManager.SwitchToMainThreadAsync();

            return factory.CreateNode(
                hierarchy.GetHierarchyItem(
                    services.GetService<SVsSolution, IVsSolution>() as IVsHierarchy, (uint)VSConstants.VSITEMID.Root))
                as ISolutionNode;
        });

        public bool IsVisible => toolWindow.Value.IsVisible;

        public void Show()
        {
            toolWindow.Value.Show();
        }

        public void Close()
        {
            toolWindow.Value.Close();
        }

        public IEnumerable<ISolutionExplorerNode> SelectedNodes =>
            asyncManager.Run(async () =>
            {
                var solution = await Solution;
                if (solution == null)
                    return Enumerable.Empty<ISolutionExplorerNode>();

                return solution.SelectedNodes;
            });
    }
}
