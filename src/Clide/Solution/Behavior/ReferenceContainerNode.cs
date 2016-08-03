using Clide.Properties;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Linq;
using VSLangProj;

namespace Clide
{
	class ReferenceContainerNode : BehaviorNode<IProjectNode, Project>, IReferenceContainerNode
	{
		readonly Lazy<IVsHierarchyItem> hierarchyNode;

		public ReferenceContainerNode(IReferencesNode node)
			: this(node.OwningProject)
		{ }

		public ReferenceContainerNode(IProjectNode node)
			: base(node)
		{
			hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
		}

		protected override Lazy<IVsHierarchyItem> HierarchyNode => hierarchyNode;

		public void AddReference(IProjectNode referencedProject)
		{
			var automationReferencedProject = referencedProject.AsVsHierarchyItem().GetExtenderObject() as Project;
			if (automationReferencedProject == null)
				throw new NotSupportedException(Strings.ProjectNode.AddProjectReferenceNotSupported(referencedProject.Name));

			var langProject = Node.AsVsLangProject();
			if (langProject == null)
				throw new NotSupportedException(Strings.ProjectNode.AddProjectReferenceNotSupported(Node.Name));

			if (referencedProject.Supports(KnownCapabilities.SharedAssetsProject))
			{
				var sharedProjectReferencesHelper = HierarchyNode.Value
					.GetServiceProvider()
					.GetService<SVsSharedProjectReferencesHelper, IVsSharedProjectReferencesHelper>();

				sharedProjectReferencesHelper.ChangeSharedProjectReferences(
					Node.AsVsHierarchy(),
					0,
					null,
					1,
					new object[] { referencedProject.AsVsHierarchy() });
			}
			else
			{
				langProject.References.AddProject(automationReferencedProject);

				var reference = langProject
					.References
					.OfType<Reference>()
					.Where(x => x.Name == referencedProject.Name)
					.FirstOrDefault();

				if (reference == null)
					throw new InvalidOperationException(
						Strings.ProjectNode.AddProjectReferenceFailed(referencedProject.Name, Node.Name));
			}
		}
	}
}