#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Solution.Adapters
{
    using Clide.Patterns.Adapter;
    using Clide.Sdk.Solution;
    using Clide.Solution.Implementation;
    using Microsoft.Build.Evaluation;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.IO;
    using System.Linq;

    [Adapter]
    internal class MsBuildAdapter :
        IAdapter<ProjectNode, Project>,
        IAdapter<ItemNode, ProjectItem>,
        IAdapter<Project, IProjectNode>, 
        IAdapter<ProjectItem, IItemNode>,
        IAdapter<EnvDTE.Project, Project>,
        IAdapter<EnvDTE.ProjectItem, ProjectItem>
    {
        IVsSolution vsSolution;
        ISolutionExplorerNodeFactory nodeFactory;
        ISolutionExplorer solutionExplorer;

        public MsBuildAdapter(IVsSolution vsSolution, ISolutionExplorerNodeFactory nodeFactory, ISolutionExplorer solutionExplorer)
        {
            this.vsSolution = vsSolution;
            this.nodeFactory = nodeFactory;
            this.solutionExplorer = solutionExplorer;
        }

        public Project Adapt(ProjectNode from)
        {
            return from == null || from.Project.Value == null ? null :
                ProjectCollection.GlobalProjectCollection
                    .GetLoadedProjects(from.Project.Value.FullName)
                    .FirstOrDefault();
        }

        public ProjectItem Adapt(ItemNode from)
        {
            if (from == null || from.Item.Value == null || from.Item.Value.ContainingProject == null)
                return null;

            var item = from.Item.Value;
            var itemType = (string)item.Properties.Item("ItemType").Value;
            var itemFullPath = new FileInfo(item.FileNames[1]).FullName;

            var projectName = item.ContainingProject.FullName;
            var projectDir = Path.GetDirectoryName(projectName);
            var project = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectName).FirstOrDefault();

            if (project != null)
            {
                return project.ItemsIgnoringCondition
                    .Where(i => i.ItemType == itemType)
                    .Select(i => new { Item = i, FullPath = new FileInfo(Path.Combine(projectDir, i.EvaluatedInclude)).FullName })
                    .Where(i => i.FullPath == itemFullPath)
                    .Select(i => i.Item)
                    .FirstOrDefault();
            }

            return null;
        }

        public IProjectNode Adapt(Project from)
		{
			var id = from.GetPropertyValue("ProjectGuid");
            
            // Fast path first.
            var guid = Guid.Empty;
            IVsHierarchy hierarchy;
            if (!String.IsNullOrEmpty(id) && Guid.TryParse(id, out guid) && 
                ErrorHandler.Succeeded(vsSolution.GetProjectOfGuid(ref guid, out hierarchy)))
            {
                return (IProjectNode)nodeFactory.Create(new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT));
            }

            // Slow way next
            return solutionExplorer.Solution.FindProject(x => x.PhysicalPath.Equals(from.FullPath, StringComparison.OrdinalIgnoreCase));
		}

        public IItemNode Adapt(ProjectItem from)
        {
			var id = from.Project.GetPropertyValue("ProjectGuid");
            
            // Fast path first.
            var guid = Guid.Empty;
            IVsHierarchy hierarchy = null;
            if (String.IsNullOrEmpty(id) || !Guid.TryParse(id, out guid) ||
                !ErrorHandler.Succeeded(vsSolution.GetProjectOfGuid(ref guid, out hierarchy)))
            {
                // Slow way next
                var project = solutionExplorer.Solution
                    .FindProject(x => x.PhysicalPath.Equals(from.Project.FullPath, StringComparison.OrdinalIgnoreCase));
                if (project != null)
                    hierarchy = project.As<IVsHierarchy>();
            }
            
            if (hierarchy == null)
                return null;

            uint itemId;
            if (!ErrorHandler.Succeeded(hierarchy.ParseCanonicalName(from.GetMetadataValue("FullPath"), out itemId)))
                return null;

            return nodeFactory.Create(new VsSolutionHierarchyNode(hierarchy, itemId)) as IItemNode;
        }

		public Project Adapt (EnvDTE.Project from)
		{
            return from == null ? null :
                ProjectCollection.GlobalProjectCollection
                    .GetLoadedProjects(from.FullName)
                    .FirstOrDefault();
		}

		public ProjectItem Adapt (EnvDTE.ProjectItem from)
		{
			var project = Adapt (from.ContainingProject);
			if (project == null || from.FileCount == 0 || string.IsNullOrEmpty(from.FileNames[1]))
				return null;

			var fromFile = new FileInfo(from.FileNames[1]).FullName;

			return project.AllEvaluatedItems.FirstOrDefault (item =>
				new FileInfo (item.GetMetadataValue ("FullPath")).FullName == fromFile);
		}
	}
}