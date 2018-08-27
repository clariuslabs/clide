using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
namespace Clide.Adapters
{

    [Adapter]
    internal class MsBuildAdapter :
        IAdapter<EnvDTE.Project, Project>,
        IAdapter<ProjectNode, Project>,
        IAdapter<Project, IProjectNode>
    {
        readonly Lazy<IVsSolution> vsSolution;
        readonly Lazy<ISolutionExplorerNodeFactory> nodeFactory;
        readonly Lazy<ISolutionExplorer> solutionExplorer;
        readonly Lazy<IVsHierarchyItemManager> hierarchyItemManager;

        [ImportingConstructor]
        public MsBuildAdapter(
            [Import(ContractNames.Interop.VsSolution)] Lazy<IVsSolution> vsSolution,
            Lazy<ISolutionExplorerNodeFactory> nodeFactory,
            Lazy<ISolutionExplorer> solutionExplorer,
            [Import(ContractNames.Interop.IVsHierarchyItemManager)] Lazy<IVsHierarchyItemManager> hierarchyItemManager)
        {
            this.vsSolution = vsSolution;
            this.nodeFactory = nodeFactory;
            this.solutionExplorer = solutionExplorer;
            this.hierarchyItemManager = hierarchyItemManager;
        }

        public Project Adapt(EnvDTE.Project from) =>
            from == null || from.FullName == null ? null :
                ProjectCollection.GlobalProjectCollection
                    .GetLoadedProjects(from.FullName)
                    .FirstOrDefault();

        public Project Adapt(ProjectNode from) =>
            from == null || from.PhysicalPath == null ? null :
                ProjectCollection.GlobalProjectCollection
                    .GetLoadedProjects(from.PhysicalPath)
                    .FirstOrDefault();

        public IProjectNode Adapt(Project from)
        {
            var id = from.GetPropertyValue("ProjectGuid");

            // Fast path first.
            var guid = Guid.Empty;
            IVsHierarchy hierarchy;
            if (!String.IsNullOrEmpty(id) && Guid.TryParse(id, out guid) &&
                ErrorHandler.Succeeded(vsSolution.Value.GetProjectOfGuid(ref guid, out hierarchy)))
            {
                return (IProjectNode)nodeFactory.Value.CreateNode(hierarchyItemManager.Value.GetHierarchyItem(hierarchy, VSConstants.VSITEMID_ROOT));
            }

            // Slow way next
            return solutionExplorer.Value.Solution.FindProject(x => x.PhysicalPath.Equals(from.FullPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}