using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
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
        readonly JoinableLazy<IVsSolution> vsSolution;
        readonly Lazy<ISolutionExplorerNodeFactory> nodeFactory;
        readonly Lazy<ISolutionExplorer> solutionExplorer;
        readonly JoinableLazy<IVsHierarchyItemManager> hierarchyItemManager;
        readonly JoinableTaskFactory asyncManager;

        [ImportingConstructor]
        public MsBuildAdapter(
            JoinableLazy<IVsSolution> vsSolution,
            Lazy<ISolutionExplorerNodeFactory> nodeFactory,
            Lazy<ISolutionExplorer> solutionExplorer,
            JoinableLazy<IVsHierarchyItemManager> hierarchyItemManager,
            JoinableTaskContext jtc)
        {
            this.vsSolution = vsSolution;
            this.nodeFactory = nodeFactory;
            this.solutionExplorer = solutionExplorer;
            this.hierarchyItemManager = hierarchyItemManager;
            this.asyncManager = jtc.Factory;
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
                ErrorHandler.Succeeded(vsSolution.GetValue().GetProjectOfGuid(ref guid, out hierarchy)))
            {
                return (IProjectNode)nodeFactory.Value.CreateNode(hierarchyItemManager.GetValue().GetHierarchyItem(hierarchy, VSConstants.VSITEMID_ROOT));
            }

            // Slow way next
            return asyncManager.Run(async () =>
                (await solutionExplorer.Value.Solution)
                    .FindProject(x => x.PhysicalPath.Equals(from.FullPath, StringComparison.OrdinalIgnoreCase)));
        }
    }
}