using System;
using System.ComponentModel.Composition;

namespace Clide
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : ExportAttribute
    {
        /// <summary>
        /// Creates an instance of <see cref="CommandAttribute"/>
        /// </summary>
        /// <param name="packageGuid">The Package Guid</param>
        /// <param name="groupGuid">The CommandSet Guid</param>
        /// <param name="commandId">The Command ID</param>
        /// <param name="visibilityContextGuid">
        /// Gets the associated UI Context guid.
        /// When the UI Context is not active, the QueryStatus or Execute method won't be invoked at all.
        /// And the command will be disabled and invisible.
        /// </param>
        public CommandAttribute(string packageGuid, string groupGuid, int commandId, string visibilityContextGuid = null)
            : base(typeof(ICommandExtension))
        {
            PackageId = packageGuid;
            GroupId = groupGuid;
            CommandId = commandId;
            VisibilityContextGuid = visibilityContextGuid;
        }

        /// <summary>
        /// Gets the Package Guid
        /// </summary>
        public string PackageId { get; }

        /// <summary>
        /// Gets the CommandSet Guid
        /// </summary>
        public string GroupId { get; }

        /// <summary>
        /// Gets the Command ID
        /// </summary>
        public int CommandId { get; }

        /// <summary>
        /// Gets the associated UI Context guid.
        /// When the UI Context is not active, the QueryStatus or Execute method won't be invoked at all.
        /// And the command will be disabled and invisible.
        /// </summary>
        public string VisibilityContextGuid { get; }
    }
}