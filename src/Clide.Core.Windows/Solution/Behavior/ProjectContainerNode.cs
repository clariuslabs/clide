using Clide.Properties;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;

namespace Clide
{
    class ProjectContainerNode : IProjectContainerNode
    {
        ISolutionNode solutionNode;
        Lazy<IVsHierarchyItem> hierarchyNode;

        public ProjectContainerNode(ISolutionNode solutionNode)
        {
            this.solutionNode = solutionNode;
            hierarchyNode = new Lazy<IVsHierarchyItem>(() => solutionNode.AsVsHierarchyItem());
        }

        public IProjectNode UnfoldTemplate(string templateId, string projectName, string language = "CSharp")
        {
            var solution = hierarchyNode.Value.GetExtenderObject<Solution2>();

            var projectTemplatePath = solution.GetProjectTemplate(templateId, language);
            if (projectTemplatePath == null)
                throw new NotSupportedException(
                    Strings.ProjectContainerNode.TemplateNotFound(templateId, language));

            solution.AddFromTemplate(
                projectTemplatePath,
                Path.Combine(Path.GetDirectoryName(solutionNode.PhysicalPath), projectName),
                projectName);

            var project = solutionNode.FindProject(x => x.Name == projectName);

            if (project == null)
                throw new InvalidOperationException(
                    Strings.ProjectContainerNode.UnfoldTemplateFailed(templateId, language));

            return project;
        }
    }
}
