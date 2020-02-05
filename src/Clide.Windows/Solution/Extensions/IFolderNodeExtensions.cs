namespace Clide
{
    public static partial class SolutionExtensions
    {
        public static IFolderNode CreateFolder(this IFolderNode folder, string name) =>
            folder.AsContainerNode().CreateFolder(name);

        public static IProjectItemNode AddItem(this IFolderNode folder, string path) =>
            folder.AsContainerNode().AddItem(path);

        public static void Delete(this IFolderNode folder) =>
            folder.AsDeletableNode().Delete();

        public static void Remove(this IFolderNode folder) =>
            folder.AsRemovableNode().Remove();
    }
}
