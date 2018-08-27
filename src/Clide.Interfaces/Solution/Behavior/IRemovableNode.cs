namespace Clide
{
    /// <summary>
    /// Represents a node that can be removed
    /// </summary>
    public interface IRemovableNode
    {
        /// <summary>
        /// Removes the node from its containing parent and keeps the physical item
        /// </summary>
        void Remove();
    }
}