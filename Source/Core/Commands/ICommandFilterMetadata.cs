namespace Clide.Commands
{
    using System.ComponentModel;

    /// <summary>
    /// Metadata associated with command filters that are exported to the environment using 
    /// the <see cref="CommandFilterAttribute"/>, for use in combination with 
    /// <see cref="ICommandManager.AddFilters"/> .
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ICommandFilterMetadata : ICommandMetadata
    {
        /// <summary>
        /// Gets the GUID of the package that exposes the filters, 
        /// which can be different than the <see cref="ICommandMetadata.PackageId"/> 
        /// which is instead the GUID of the package that provides the original 
        /// command.
        /// </summary>
        string OwningPackageId { get; }
    }
}
