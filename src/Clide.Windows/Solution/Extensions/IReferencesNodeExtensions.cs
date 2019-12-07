namespace Clide
{
    public static partial class SolutionExtensions
    {
        public static void AddReference(this IReferencesNode references, IProjectNode projectReference) =>
            references.AsReferenceContainerNode().AddReference(projectReference);
    }
}