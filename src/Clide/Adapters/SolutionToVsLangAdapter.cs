using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using VSLangProj;

namespace Clide.Adapters
{
	[Adapter]
	class SolutionToVsLangAdapter :
		IAdapter<ProjectNode, VSProject>,
		IAdapter<ItemNode, VSProjectItem>,
		IAdapter<ReferenceNode, Reference>,
		IAdapter<ReferencesNode, References>
	{
		public VSProject Adapt (ProjectNode from)
		{
			var project = from.HierarchyNode.GetExtenderObject() as Project;

			return project == null ? null : project.Object as VSProject;
		}

		public VSProjectItem Adapt (ItemNode from)
		{
			var item = from.HierarchyNode.GetExtenderObject() as ProjectItem;

			return item == null ? null : item.Object as VSProjectItem;
		}

		public Reference Adapt (ReferenceNode from) => from.HierarchyNode.GetExtenderObject () as Reference;

		public References Adapt (ReferencesNode from)
		{
			var project = from.OwningProject.As<VSProject>();
			if (project == null)
				return null;

			return project.References;
		}
	}
}
