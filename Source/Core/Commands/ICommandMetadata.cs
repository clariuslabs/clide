namespace Clide.Commands
{
    using System.ComponentModel;

    /// <summary>
    /// Metadata associated with commands that are exported to the environment using 
    /// the <see cref="CommandAttribute"/>, for use in combination with 
    /// <see cref="ICommandManager.AddCommands"/> .
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ICommandMetadata
    {
        /// <summary>
        /// Gets the owning package GUID.
        /// </summary>
        string PackageId { get; }

        /// <summary>
        /// Gets the group id for the command.
        /// </summary>
        string GroupId { get; }

        /// <summary>
        /// Gets the command id.
        /// </summary>
        int CommandId { get; }
    }
}
