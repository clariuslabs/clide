using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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

        [ImportingConstructor]
        public SolutionExplorerNodeFactory(
            [ImportMany(ContractNames.FallbackNodeFactory)] IEnumerable<ICustomSolutionExplorerNodeFactory> defaultFactories,
            [ImportMany] IEnumerable<ICustomSolutionExplorerNodeFactory> customFactories,
            IAdapterService adapter,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer)
        {
            this.defaultFactories = defaultFactories.ToList();
            this.customFactories = customFactories.ToList();
            this.adapter = adapter;
            this.solutionExplorer = solutionExplorer;
        }

        public ISolutionExplorerNode CreateNode(IVsHierarchyItem item)
        {
            if (item == null)
                return null;

            var factory = customFactories.FirstOrDefault(f => f.Supports(item)) ??
                defaultFactories.FirstOrDefault(f => f.Supports(item));

            return factory == null ? new GenericNode(item, this, adapter, solutionExplorer) : factory.CreateNode(item);
        }
    }
}
