namespace Clide.Commands
{
    using System;
    using System.ComponentModel.Composition;

    /// <summary>
    /// Attribute that must be placed on command implementations in order to 
    /// use the <see cref="ICommandManager.AddCommands"/> method.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandFilterAttribute : InheritedExportAttribute, ICommandFilterMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandFilterAttribute"/> class.
        /// </summary>
        public CommandFilterAttribute(string owningPackageGuid, string commandPackageGuid, string groupGuid, int commandId)
            : base(typeof(ICommandFilter))
        {
            this.OwningPackageId = owningPackageGuid;
            this.PackageId = commandPackageGuid;
            this.GroupId = groupGuid;
            this.CommandId = commandId;
        }

        /// <summary>
        /// Gets the package GUID.
        /// </summary>
        public string PackageId { get; private set; }

        /// <summary>
        /// Gets the group GUID.
        /// </summary>
        public string GroupId { get; private set; }

        /// <summary>
        /// Gets the command id.
        /// </summary>
        public int CommandId { get; private set; }

        /// <summary>
        /// Gets the owning package GUID.
        /// </summary>
        public string OwningPackageId { get; private set; }
    }
}
