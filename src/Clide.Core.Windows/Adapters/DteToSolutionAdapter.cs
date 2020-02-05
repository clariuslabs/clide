using Clide.Sdk;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
namespace Clide
{

    [Adapter]
    internal class DteToSolutionAdapter :
        IAdapter<Solution, ISolutionNode>,
        IAdapter<Project, IProjectNode>,
        IAdapter<ProjectItem, IItemNode>
    {
        readonly JoinableLazy<IVsSolution> vsSolution;
        readonly Lazy<ISolutionExplorerNodeFactory> nodeFactory;
        readonly JoinableLazy<IVsHierarchyItemManager> hierarchyItemManager;

        [ImportingConstructor]
        public DteToSolutionAdapter(
            JoinableLazy<IVsSolution> vsSolution,
            Lazy<ISolutionExplorerNodeFactory> nodeFactory,
            JoinableLazy<IVsHierarchyItemManager> hierarchyItemManager)
        {
            this.vsSolution = vsSolution;
            this.nodeFactory = nodeFactory;
            this.hierarchyItemManager = hierarchyItemManager;
        }

        public ISolutionNode Adapt(Solution from) =>
            nodeFactory
                .Value
                .CreateNode(
                    hierarchyItemManager.GetValue().GetHierarchyItem(
                        vsSolution.GetValue() as IVsHierarchy, VSConstants.VSITEMID_ROOT)) as ISolutionNode;

        public IProjectNode Adapt(Project from)
        {
            var uniqueName = "";
            try
            {
                // This might throw if the project isn't loaded yet.
                uniqueName = from.UniqueName;
            }
            catch (Exception)
            {
                // As a fallback, in C#/VB, the UniqueName == FullName.
                // It may still fail in the ext call though, but we do our best
                uniqueName = from.FullName;
            }

            IVsHierarchy project;

            if (!ErrorHandler.Succeeded(vsSolution.GetValue()
                .GetProjectOfUniqueName(uniqueName, out project)))
                return null;

            return nodeFactory.Value
                .CreateNode(hierarchyItemManager.GetValue().GetHierarchyItem(project, VSConstants.VSITEMID_ROOT))
                as IProjectNode;
        }

        public IItemNode Adapt(ProjectItem from)
        {
            IVsHierarchy project;

            if (!ErrorHandler.Succeeded(vsSolution.GetValue()
                .GetProjectOfUniqueName(from.ContainingProject.UniqueName, out project)))
                return null;

            var fileName = from.FileNames[0];
            var found = 0;
            uint itemId = 0;

            if (!ErrorHandler.Succeeded(((IVsProject)project).IsDocumentInProject(
                fileName, out found, new VSDOCUMENTPRIORITY[1], out itemId)) ||
                found == 0 || itemId == 0)
                return null;

            return nodeFactory.Value
                .CreateNode(hierarchyItemManager.GetValue().GetHierarchyItem(project, itemId))
                as IItemNode;
        }
    }
}
