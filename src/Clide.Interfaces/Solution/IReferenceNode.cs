namespace Clide
{
    /// <summary>
    /// Interface implemented by a reference in a project.
    /// </summary>
    public interface IReferenceNode : IProjectItemNode
    {
        /// <summary>
        /// Gets the referenced project when the instance is a project reference
        /// </summary>
        IProjectNode SourceProject { get; }
    }
}