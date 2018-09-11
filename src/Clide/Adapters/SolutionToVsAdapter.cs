using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
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

        IVsProject IAdapter<ProjectNode, IVsProject>.Adapt(ProjectNode from) => from.HierarchyNode.GetActualHierarchy() as IVsProject;

        IVsBuildPropertyStorage IAdapter<ProjectNode, IVsBuildPropertyStorage>.Adapt(ProjectNode from)
        {
            var result = from.InnerHierarchyNode?.GetActualHierarchy() as IVsBuildPropertyStorage;
            if (result == null)
            {
                var hierarchy = from.HierarchyNode.GetActualHierarchy();

                result = hierarchy as IVsBuildPropertyStorage;
                if (result == null && hierarchy is FlavoredProjectBase)
                {
                    var innerVsHierarchyField = hierarchy
                        .GetType()
                        .GetField("_innerVsHierarchy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    if (innerVsHierarchyField != null)
                        result = innerVsHierarchyField.GetValue(hierarchy) as IVsBuildPropertyStorage;
                }
            }

            return result;
        }
    }
}