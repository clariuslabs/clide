using System.ComponentModel.Composition;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Clide
{
	[Adapter]
	class ProjectToDte : IAdapter<ProjectNode, Project>
	{
		public Project Adapt (ProjectNode from) => from.HierarchyNode.GetExtenderObject () as Project;
	}
}