using Clide.Properties;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;
using VSLangProj;

namespace Clide
{
    class ReferenceContainerNode : IReferenceContainerNode
    {
        readonly IProjectNode node;
        readonly Lazy<IVsHierarchyItem> hierarchyNode;

        public ReferenceContainerNode(IReferencesNode node)
            : this(node.OwningProject)
        { }

        public ReferenceContainerNode(IProjectNode node)
        {
            this.node = node;

            hierarchyNode = new Lazy<IVsHierarchyItem>(() => node.AsVsHierarchyItem());
        }

        public void AddReference(IProjectNode referencedProject)
        {
            if (referencedProject.Supports(KnownCapabilities.SharedAssetsProject))
            {
                var sharedProjectReferencesHelper = hierarchyNode.Value
                    .GetServiceProvider()
                    .GetService<SVsSharedProjectReferencesHelper, IVsSharedProjectReferencesHelper>();

                sharedProjectReferencesHelper.ChangeSharedProjectReferences(
                    node.AsVsHierarchy(),
                    0,
                    null,
                    1,
                    new object[] { referencedProject.AsVsHierarchy() });

                sharedProjectReferencesHelper.ChangeSharedMSBuildFileImports(
                    node.AsVsHierarchy(),
                    new[] { referencedProject.PhysicalPath },
                    new[] { Path.ChangeExtension(referencedProject.PhysicalPath, ".projitems") },
                    "Shared"
                    );
            }
            else
            {
                var automationReferencedProject = referencedProject.AsVsHierarchyItem().GetExtenderObject() as Project;
                if (automationReferencedProject == null)
                    throw new NotSupportedException(Strings.ProjectNode.AddProjectReferenceNotSupported(referencedProject.Name));

                var langProject = node.AsVsLangProject();
                if (langProject == null)
                    throw new NotSupportedException(Strings.ProjectNode.AddProjectReferenceNotSupported(node.Name));

                langProject.References.AddProject(automationReferencedProject);

                var reference = langProject
                    .References
                    .OfType<Reference>()
                    .Where(x => x.Name == referencedProject.Name)
                    .FirstOrDefault();

                if (reference == null)
                    throw new InvalidOperationException(
                        Strings.ProjectNode.AddProjectReferenceFailed(referencedProject.Name, node.Name));
            }
        }
    }
}