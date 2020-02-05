using System.Linq;
using Clide.Sdk;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xunit;

namespace Clide.Solution
{
    [Trait("LongRunning", "true")]
    [Trait("Feature", "Solution Traversal")]
    public abstract class NodeFactorySpec<TNode>
    {
        DTE dte;
        IVsHierarchyItem solution;

        protected NodeFactorySpec()
        {
            dte = GlobalServices.GetService<DTE>();
            solution = GlobalServices
                .GetService<SComponentModel, IComponentModel>()
                .GetService<IVsHierarchyItemManager>()
                .GetHierarchyItem(
                    GlobalServices.GetService<SVsSolution, IVsSolution>() as IVsHierarchy,
                    (uint)VSConstants.VSITEMID.Root);
        }

        public virtual void when_item_is_supported_then_factory_supports_it(string relativePath)
        {
            when_item_is_supported_then_factory_supports_it(relativePath, null);
        }

        public virtual void when_item_is_supported_then_factory_supports_it(string relativePath, string minimumVersion)
        {
            // Skip assertions if the VS is lower than the minimum version.
            if (!string.IsNullOrEmpty(minimumVersion) && GlobalServices.GetService<DTE>().Version.CompareTo(minimumVersion) == -1)
                return;

            var item = solution.NavigateToItem(relativePath);
            Assert.True(item != null, string.Format("Failed to locate solution element at {0} in solution {1}.", relativePath, solution.CanonicalName));

            var factory = GetFactory();

            Assert.True(factory.Supports(item));

            var node = factory.CreateNode(item);
            Assert.NotNull(node);
        }

        public virtual void when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(string relativePath)
        {
            when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(relativePath, null);
        }

        public virtual void when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(string relativePath, string minimumVersion)
        {
            // Skip assertions if the VS is lower than the minimum version.
            if (!string.IsNullOrEmpty(minimumVersion) && dte.Version.CompareTo(minimumVersion) == -1)
                return;

            var item = solution.NavigateToItem(relativePath);
            Assert.True(item != null, string.Format("Failed to locate solution element at {0} in solution {1}.", relativePath, solution.CanonicalName));

            var factory = GetFactory();

            Assert.False(factory.Supports(item));

            var node = factory.CreateNode(item);
            Assert.Null(node);
        }

        ICustomSolutionExplorerNodeFactory GetFactory()
        {
            return GlobalServices.GetService<SComponentModel, IComponentModel>().DefaultExportProvider
                .GetExportedValues<ICustomSolutionExplorerNodeFactory>(ContractNames.FallbackNodeFactory)
                .First(factory => factory.GetType().Name == typeof(TNode).Name);
        }
    }
}
