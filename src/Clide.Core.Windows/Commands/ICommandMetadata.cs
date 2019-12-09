using System;

namespace Clide.Commands
{
    /// <summary>
    /// Represents the metadata of an exported command
    /// </summary>
    public interface ICommandMetadata
    {
        /// <summary>
        /// Gets the package guid the command belongs to
        /// </summary>
        string PackageId { get; }

        /// <summary>
        /// Gets the command group id the commands belongs to
        /// </summary>
        string GroupId { get; }

        /// <summary>
        /// Gets the command id
        /// </summary>
        int CommandId { get; }

        /// <summary>
        /// Gets the UI Context Guid that should be active when the command is executed
        /// </summary>
        string VisibilityContextGuid { get; }
    }
}