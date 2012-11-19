#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
namespace System
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.Globalization;
    using Microsoft.VisualStudio.ComponentModelHost;

    /// <summary>
    /// Defines extension methods related to <see cref="IServiceProvider"/> for use within Visual Studio.
    /// </summary>
    internal static class ComponentModelExtensions
    {
        public static Lazy<T> GetExport<T>(this IServiceProvider provider)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExport<T>();
        }

        public static Lazy<T, TMetadataView> GetExport<T, TMetadataView>(this IServiceProvider provider)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExport<T, TMetadataView>();
        }

        public static Lazy<T> GetExport<T>(this IServiceProvider provider, string contractName)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExport<T>(contractName);
        }

        public static Lazy<T, TMetadataView> GetExport<T, TMetadataView>(this IServiceProvider provider, string contractName)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExport<T, TMetadataView>(contractName);
        }

        public static T GetExportedValue<T>(this IServiceProvider provider)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExportedValue<T>();
        }

        public static T GetExportedValue<T>(this IServiceProvider provider, string contractName)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExportedValue<T>(contractName);
        }

        public static T GetExportedValueOrDefault<T>(this IServiceProvider provider)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExportedValueOrDefault<T>();
        }

        public static T GetExportedValueOrDefault<T>(this IServiceProvider provider, string contractName)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExportedValueOrDefault<T>(contractName);
        }

        public static IEnumerable<T> GetExportedValues<T>(this IServiceProvider provider)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExportedValues<T>();
        }

        public static IEnumerable<T> GetExportedValues<T>(this IServiceProvider provider, string contractName)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExportedValues<T>(contractName);
        }

        public static IEnumerable<Lazy<T>> GetExports<T>(this IServiceProvider provider)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExports<T>();
        }

        public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>(this IServiceProvider provider)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExports<T, TMetadataView>();
        }

        public static IEnumerable<Lazy<T>> GetExports<T>(this IServiceProvider provider, string contractName)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExports<T>(contractName);
        }

        public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>(this IServiceProvider provider, string contractName)
        {
            return GetGlobalExportProviderOrThrow(provider).GetExports<T, TMetadataView>(contractName);
        }

        private static ExportProvider GetGlobalExportProviderOrThrow(IServiceProvider provider)
        {
            var components = provider.GetService<SComponentModel, IComponentModel>();
            if (components == null)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Service '{0}' is required.",
                    typeof(ExportProvider)));
            }

            return components.DefaultExportProvider;
        }
    }
}