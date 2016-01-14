using System.ComponentModel.Composition;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Clide
{
	[Export (typeof (IAdapter))]
	class ProjectToDte : IAdapter<ProjectNode, Project>
	{
		public Project Adapt (ProjectNode from)
		{
			return from.HierarchyNode.GetExtenderObject () as Project;
		}
	}
}