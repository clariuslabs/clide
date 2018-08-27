using System;
using System.Collections.Generic;

namespace Clide
{
    /// <summary>
    /// The generic Service Locator interface. This interface is used
    /// to retrieve services (instances identified by type and optional
    /// name) from a container. It extends <see cref="IServiceProvider"/> 
    /// to provide instance retrieval of both Visual Studio services (via 
    /// <see cref="IServiceProvider.GetService(Type)"/>) as well as VS MEF
    /// composition container via the added <c>GetExport</c> and 
    /// <c>GetExports</c> members.
    /// </summary>
    public interface IServiceLocator : IServiceProvider
    {
        /// <summary>
        /// Get an instance of the given <paramref name="contractType"/> from the 
        /// Visual Studio composition container.
        /// </summary>
        /// <param name="contractType">Type of object requested.</param>
        /// <param name="contractName">Optional name the object was registered with.</param>
        /// <exception cref="MissingDependencyException">if there is an error resolving
        /// the service instance.</exception>
        /// <returns>The requested service instance.</returns>
        object GetExport(Type contractType, string contractName = null);

        /// <summary>
        /// Get an instance of the given <paramref name="contractType"/> from the 
        /// Visual Studio composition container.
        /// </summary>
        /// <param name="contractType">Type of object requested.</param>
        /// <param name="metadataType">The type of the metadata of the <see cref="System.Lazy{T, TMetadata}" /> objects to return.</param>
        /// <param name="contractName">Optional name the object was registered with.</param>
        /// <exception cref="MissingDependencyException">if there is are errors resolving
        /// the service instance.</exception>
        /// <returns>A sequence of instances of the requested <paramref name="contractType"/>.</returns>
        Lazy<object, object> GetExport(Type contractType, Type metadataType, string contractName = null);

        /// <summary>
        /// Get all instances of the given <paramref name="contractType"/> currently
        /// registered in the Visual Studio composition container.
        /// </summary>
        /// <param name="contractType">Type of object requested.</param>
        /// <param name="contractName">Optional name the object was registered with.</param>
        /// <exception cref="MissingDependencyException">if there is are errors resolving
        /// the service instance.</exception>
        /// <returns>A sequence of instances of the requested <paramref name="contractType"/>.</returns>
        IEnumerable<object> GetExports(Type contractType, string contractName = null);

        /// <summary>
        /// Get all instances of the given <paramref name="contractType"/> currently
        /// registered in the Visual Studio composition container.
        /// </summary>
        /// <param name="contractType">Type of object requested.</param>
        /// <param name="metadataType">The type of the metadata of the <see cref="System.Lazy{T, TMetadata}" /> objects to return.</param>
        /// <param name="contractName">Optional name the object was registered with.</param>
        /// <exception cref="MissingDependencyException">if there is are errors resolving
        /// the service instance.</exception>
        /// <returns>A sequence of instances of the requested <paramref name="contractType"/>.</returns>
        IEnumerable<Lazy<object, object>> GetExports(Type contractType, Type metadataType, string contractName = null);
    }
}
