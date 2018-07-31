using EnvDTE;

namespace Clide
{
    [Adapter]
    class ProjectToDte : IAdapter<ProjectNode, Project>
    {
        public Project Adapt(ProjectNode from) => from.HierarchyNode.GetExtenderObject() as Project;
    }
}