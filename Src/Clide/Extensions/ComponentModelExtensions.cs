#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace System
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.Globalization;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Clide;
    using System.ComponentModel.Composition;
    using Clide.Properties;

    /// <summary>
    /// Defines extension methods related to <see cref="IServiceProvider"/> for use within Visual Studio.
    /// </summary>
    public static class ComponentModelExtensions
    {
        /// <summary>
        /// Composes the specified part by using the specified composition service.
        /// </summary>
        /// <param name="compositionService">The composition service to use.</param>
        /// <param name="attributedPart">The part to compose.</param>
        public static void SatisfyImportsOnce(this ExportProvider provider, object attributedPart)
        {
            var composition = provider as ICompositionService;
            if (composition == null)
                throw new InvalidOperationException(Strings.ComponentModelExtensions.ExportProviderIsNotCompositionService(provider));

            composition.SatisfyImportsOnce(attributedPart);
        }

        /// <summary>Returns the export with the contract name derived from the specified type parameter. If there is not exactly one matching export, an exception is thrown.</summary>
        /// <returns>The export with the contract name derived from the specified type parameter.</returns>
        /// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`1" /> object to return. The contract name is also derived from this type parameter.</typeparam>
        /// <param name="provider">The hosting package.</param>
        /// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero <see cref="T:System.Lazy`1" /> objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.-or-There is more than one <see cref="T:System.Lazy`1" /> object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        public static Lazy<T> GetExport<T>(this IServiceProvider provider)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExport<T>();
        }

        /// <summary>Returns the export with the contract name derived from the specified type parameter. If there is not exactly one matching export, an exception is thrown.</summary>
        /// <returns>System.Lazy`2</returns>
        /// <param name="provider">The hosting package.</param>
        /// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`2" /> object to return. The contract name is also derived from this type parameter.</typeparam>
        /// <typeparam name="TMetadataView">The type of the metadata view of the <see cref="T:System.Lazy`2" /> object to return.</typeparam>
        /// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero <see cref="T:System.Lazy`2" /> objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.-or-There is more than one <see cref="T:System.Lazy`2" /> object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="TMetadataView" /> is not a valid metadata view type.</exception>
        public static Lazy<T, TMetadataView> GetExport<T, TMetadataView>(this IServiceProvider provider)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExport<T, TMetadataView>();
        }

        /// <summary>Returns the export with the specified contract name. If there is not exactly one matching export, an exception is thrown.</summary>
        /// <returns>The export with the specified contract name.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <param name="contractName">The contract name of the <see cref="T:System.Lazy`1" /> object to return, or null or an empty string ("") to use the default contract name.</param>
        /// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`1" /> object to return.</typeparam>
        /// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero <see cref="T:System.Lazy`1" /> objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.-or-There is more than one <see cref="T:System.Lazy`1" /> object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        public static Lazy<T> GetExport<T>(this IServiceProvider provider, string contractName)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExport<T>(contractName);
        }

        /// <summary>Returns the export with the specified contract name. If there is not exactly one matching export, an exception is thrown.</summary>
        /// <returns>The export with the specified contract name.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <param name="contractName">The contract name of the <see cref="T:System.Lazy`2" /> object to return, or null or an empty string ("") to use the default contract name.</param>
        /// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`2" /> object to return.</typeparam>
        /// <typeparam name="TMetadataView">The type of the metadata view of the <see cref="T:System.Lazy`2" /> object to return.</typeparam>
        /// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero <see cref="T:System.Lazy`2" /> objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.-or-There is more than one <see cref="T:System.Lazy`2" /> object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="TMetadataView" /> is not a valid metadata view type.</exception>
        public static Lazy<T, TMetadataView> GetExport<T, TMetadataView>(this IServiceProvider provider, string contractName)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExport<T, TMetadataView>(contractName);
        }

        /// <summary>Returns the exported object with the contract name derived from the specified type parameter. If there is not exactly one matching exported object, an exception is thrown.</summary>
        /// <returns>The exported object with the contract name derived from the specified type parameter.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <typeparam name="T">The type of the exported object to return. The contract name is also derived from this type parameter.</typeparam>
        /// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero exported objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.-or-There is more than one exported object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">The underlying exported object cannot be cast to <paramref name="T" />.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
        public static T GetExportedValue<T>(this IServiceProvider provider)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExportedValue<T>();
        }

        /// <summary>Returns the exported object with the specified contract name. If there is not exactly one matching exported object, an exception is thrown.</summary>
        /// <returns>The exported object with the specified contract name.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <param name="contractName">The contract name of the exported object to return, or null or an empty string ("") to use the default contract name.</param>
        /// <typeparam name="T">The type of the exported object to return.</typeparam>
        /// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero exported objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.-or-There is more than one exported object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">The underlying exported object cannot be cast to <paramref name="T" />.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
        public static T GetExportedValue<T>(this IServiceProvider provider, string contractName)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExportedValue<T>(contractName);
        }

        /// <summary>Gets the exported object with the contract name derived from the specified type parameter or the default value for the specified type, or throws an exception if there is more than one matching exported object.</summary>
        /// <returns>The exported object with the contract name derived from <paramref name="T" />, if found; otherwise, the default value for <paramref name="T" />.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <typeparam name="T">The type of the exported object to return. The contract name is also derived from this type parameter.</typeparam>
        /// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There is more than one exported object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">The underlying exported object cannot be cast to <paramref name="T" />.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
        public static T GetExportedValueOrDefault<T>(this IServiceProvider provider)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExportedValueOrDefault<T>();
        }

        /// <summary>Gets the exported object with the specified contract name or the default value for the specified type, or throws an exception if there is more than one matching exported object.</summary>
        /// <returns>The exported object with the specified contract name, if found; otherwise, the default value for <paramref name="T" />.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <param name="contractName">The contract name of the exported object to return, or null or an empty string ("") to use the default contract name.</param>
        /// <typeparam name="T">The type of the exported object to return.</typeparam>
        /// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There is more than one exported object with the specified contract name in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">The underlying exported object cannot be cast to <paramref name="T" />.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
        public static T GetExportedValueOrDefault<T>(this IServiceProvider provider, string contractName)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExportedValueOrDefault<T>(contractName);
        }

        /// <summary>Gets all the exported objects with the contract name derived from the specified type parameter.</summary>
        /// <returns>The exported objects with the contract name derived from the specified type parameter, if found; otherwise, an empty <see cref="T:System.Collections.ObjectModel.Collection`1" /> object.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <typeparam name="T">The type of the exported object to return. The contract name is also derived from this type parameter.</typeparam>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">One or more of the underlying exported objects cannot be cast to <paramref name="T" />.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
        public static IEnumerable<T> GetExportedValues<T>(this IServiceProvider provider)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExportedValues<T>();
        }

        /// <summary>Gets all the exported objects with the specified contract name.</summary>
        /// <returns>The exported objects with the specified contract name, if found; otherwise, an empty <see cref="T:System.Collections.ObjectModel.Collection`1" /> object.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <param name="contractName">The contract name of the exported objects to return; or null or an empty string ("") to use the default contract name.</param>
        /// <typeparam name="T">The type of the exported object to return.</typeparam>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">One or more of the underlying exported values cannot be cast to <paramref name="T" />.</exception>
        /// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
        public static IEnumerable<T> GetExportedValues<T>(this IServiceProvider provider, string contractName)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExportedValues<T>(contractName);
        }

        /// <summary>Gets all the exports with the contract name derived from the specified type parameter.</summary>
        /// <returns>The <see cref="T:System.Lazy`1" /> objects with the contract name derived from <paramref name="T" />, if found; otherwise, an empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> object.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`1" /> objects to return. The contract name is also derived from this type parameter.</typeparam>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        public static IEnumerable<Lazy<T>> GetExports<T>(this IServiceProvider provider)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExports<T>();
        }

        /// <summary>Gets all the exports with the contract name derived from the specified type parameter.</summary>
        /// <returns>The <see cref="T:System.Lazy`2" /> objects with the contract name derived from <paramref name="T" />, if found; otherwise, an empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> object.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`2" /> objects to return. The contract name is also derived from this type parameter.</typeparam>
        /// <typeparam name="TMetadataView">The type of the metadata view of the <see cref="T:System.Lazy`2" /> objects to return.</typeparam>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="TMetadataView" /> is not a valid metadata view type.</exception>
        public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>(this IServiceProvider provider)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExports<T, TMetadataView>();
        }

        /// <summary>Gets all the exports with the specified contract name.</summary>
        /// <returns>The <see cref="T:System.Lazy`1" /> objects with the specified contract name, if found; otherwise, an empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> object.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <param name="contractName">The contract name of the <see cref="T:System.Lazy`1" /> objects to return, or null or an empty string ("") to use the default contract name.</param>
        /// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`1" /> objects to return.</typeparam>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        public static IEnumerable<Lazy<T>> GetExports<T>(this IServiceProvider provider, string contractName)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExports<T>(contractName);
        }

        /// <summary>Gets all the exports with the specified contract name.</summary>
        /// <returns>The <see cref="T:System.Lazy`2" /> objects with the specified contract name if found; otherwise, an empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> object.</returns>
        /// <param name="provider">The hosting package.</param>
        /// <param name="contractName">The contract name of the <see cref="T:System.Lazy`2" /> objects to return, or null or an empty string ("") to use the default contract name.</param>
        /// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy'2" /> objects to return. The contract name is also derived from this type parameter.</typeparam>
        /// <typeparam name="TMetadataView">The type of the metadata view of the <see cref="T:System.Lazy`2" /> objects to return.</typeparam>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// <paramref name="TMetadataView" /> is not a valid metadata view type.</exception>
        public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>(this IServiceProvider provider, string contractName)
        {
            return GetDevEnvCompositionOrThrow(provider).GetExports<T, TMetadataView>(contractName);
        }

        private static ExportProvider GetDevEnvCompositionOrThrow(IServiceProvider provider)
        {
            throw new NotSupportedException();
            var devEnv = DevEnv.Get(provider);

            //return devEnv.ServiceLocator;
        }
    }
}