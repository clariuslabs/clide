namespace Clide
{
    public static partial class SolutionExtensions
    {
        public static IFolderNode CreateFolder(this IProjectNode project, string name) =>
            project.AsContainerNode().CreateFolder(name);

        public static IProjectItemNode AddItem(this IProjectNode project, string path) =>
            project.AsContainerNode().AddItem(path);

        public static void AddReference(this IProjectNode project, IProjectNode projectReference) =>
            project.AsReferenceContainerNode().AddReference(projectReference);
    }
}
