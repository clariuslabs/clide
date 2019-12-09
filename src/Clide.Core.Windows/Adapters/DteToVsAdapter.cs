using System;
using System.ComponentModel.Composition;
using Clide.Sdk;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Adapters
{
    [Adapter]
    class DteToVsAdapter :
        IAdapter<Solution, IVsSolution>,
        IAdapter<Project, IVsProject>,
        IAdapter<Project, IVsHierarchy>,
        IAdapter<Project, IVsHierarchyItem>,
        IAdapter<ProjectItem, IVsHierarchyItem>
    {
        IServiceProvider serviceProvider;
        IVsHierarchyItemManager hierarchyManager;

        [ImportingConstructor]
        public DteToVsAdapter(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [Import(ContractNames.Interop.IVsHierarchyItemManager)] IVsHierarchyItemManager hierarchyManager)
        {
            this.serviceProvider = serviceProvider;
            this.hierarchyManager = hierarchyManager;
        }

        IVsSolution IAdapter<Solution, IVsSolution>.Adapt(Solution from) => serviceProvider.GetService<SVsSolution, IVsSolution>();

        IVsProject IAdapter<Project, IVsProject>.Adapt(Project from)
        {
            IVsHierarchy project;

            if (!ErrorHandler.Succeeded(serviceProvider
                .GetService<SVsSolution, IVsSolution>()
                .GetProjectOfUniqueName(from.GetUniqueNameOrFullName(), out project)))
                return null;

            return project as IVsProject;
        }

        IVsHierarchy IAdapter<Project, IVsHierarchy>.Adapt(Project from) =>
            ((IAdapter<Project, IVsProject>)this).Adapt(from) as IVsHierarchy;

        IVsHierarchyItem IAdapter<Project, IVsHierarchyItem>.Adapt(Project from)
        {
            IVsHierarchy project;

            if (!ErrorHandler.Succeeded(serviceProvider
                .GetService<SVsSolution, IVsSolution>()
                .GetProjectOfUniqueName(from.GetUniqueNameOrFullName(), out project)))
                return null;

            return hierarchyManager.GetHierarchyItem(project, VSConstants.VSITEMID_ROOT);
        }

        IVsHierarchyItem IAdapter<ProjectItem, IVsHierarchyItem>.Adapt(ProjectItem from)
        {
            IVsHierarchy project;

            if (!ErrorHandler.Succeeded(serviceProvider
                .GetService<SVsSolution, IVsSolution>()
                .GetProjectOfUniqueName(from.ContainingProject.GetUniqueNameOrFullName(), out project)))
                return null;

            var fileName = from.FileNames[0];
            var found = 0;
            uint itemId = 0;

            if (!ErrorHandler.Succeeded(((IVsProject)project).IsDocumentInProject(
                fileName, out found, new VSDOCUMENTPRIORITY[1], out itemId)) ||
                found == 0 || itemId == 0)
                return null;

            return hierarchyManager.GetHierarchyItem(project, itemId);
        }
    }
}
