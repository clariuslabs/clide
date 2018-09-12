using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    [Adapter]
    class SolutionToVsAdapter :
        IAdapter<SolutionExplorerNode, IVsHierarchyItem>,
        IAdapter<SolutionExplorerNode, IVsHierarchy>,
        IAdapter<SolutionNode, IVsSolution>,
        IAdapter<ProjectNode, IVsProject>,
        IAdapter<ProjectNode, IVsBuildPropertyStorage>
    {
        IVsHierarchyItem IAdapter<SolutionExplorerNode, IVsHierarchyItem>.Adapt(SolutionExplorerNode from) => from?.HierarchyNode;

        IVsHierarchy IAdapter<SolutionExplorerNode, IVsHierarchy>.Adapt(SolutionExplorerNode from) => from?.HierarchyNode.GetActualHierarchy();

        IVsSolution IAdapter<SolutionNode, IVsSolution>.Adapt(SolutionNode from) => from.HierarchyNode.GetServiceProvider().GetService<SVsSolution, IVsSolution>();

        IVsProject IAdapter<ProjectNode, IVsProject>.Adapt(ProjectNode from) => GetVsService<IVsProject>(from);

        IVsBuildPropertyStorage IAdapter<ProjectNode, IVsBuildPropertyStorage>.Adapt(ProjectNode from) => GetVsService<IVsBuildPropertyStorage>(from);

        T GetVsService<T>(ProjectNode from) where T : class
        {
            var result = from.InnerHierarchyNode?.GetActualHierarchy() as T;

            if (result == null)
                result = from.HierarchyNode.GetActualHierarchy() as T;

            if (result == null && from.HierarchyNode.TryGetInnerHierarchy(out var innerHierarchy))
                result = innerHierarchy as T;

            return result;
        }
    }
}