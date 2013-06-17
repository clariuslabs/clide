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

namespace Clide.Composition
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.Linq;
    using System.Reflection;
    using Autofac.Builder;
    using Autofac.Core;

    /// <summary>
    /// Exposes MEF exports as services in the Autofac container automatically, 
    /// without the need for [Import] attributes.
    /// </summary>
    internal class CompositionSource : IRegistrationSource
    {
        private static readonly MethodInfo getExport = typeof(ExportProvider).GetMethod("GetExportedValueOrDefault", new Type[0]);
        private static readonly MethodInfo getExports = typeof(ExportProvider).GetMethod("GetExportedValues", new Type[0]);

        private ComposablePartCatalog catalog;
        private ExportProvider exports;

        public CompositionSource(ComposablePartCatalog catalog, ExportProvider exports)
        {
            this.catalog = catalog;
            this.exports = exports;
        }

        public bool IsAdapterForIndividualComponents { get { return false; } }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var swt = service as IServiceWithType;
            if (swt == null)
                yield break;

            // NOTE: removed support for enumerables since it would never return 
            // null and we'd effectively take over the resolution of ALL enumerables.
            // In addition, at this point we have no way of knowing if MEF should be 
            // resolving the import many or Autofac, so we just don't support it.
            // If the user wants MEF to resolve an ImportMany, he will have to do 
            // it in a property/field with [ImportMany] annotation, which works fine.
            // Note that deciding whether to provide the registration or not based 
            // on wether we find or not actual instances for the enumeration is also 
            // not ideal. What we'd really need is to aggregate on top of an existing 
            // registration.

            //var serviceType = GetElementType(swt.ServiceType);

            var serviceType = swt.ServiceType;
            var exportMethod = (serviceType == swt.ServiceType) ?
                // If the two are the same, this is a single export retrieval.
                getExport.MakeGenericMethod(serviceType) :
                // Otherwise, this is an import many type service.
                getExports.MakeGenericMethod(serviceType);

            // Unfortunately, VSMEF rewrote the logic of how exports are retrieved 
            // and the catalog doesn't have everything that can be retrieved, so we 
            // have to resort to actually invoking the GetExportedValueOrDefault 
            // to really know if the export is there or not :(
            var contractName = AttributedModelServices.GetContractName(swt.ServiceType);
            // We short-circuit anyways for things that are in the catalog so that we 
            // don't retrieve the part at this time.
            if (!catalog.Parts.SelectMany(part => part.ExportDefinitions).Any(e => e.ContractName.Equals(contractName)) || 
                exportMethod.Invoke(exports, null) == null)
                yield break;
            
            yield return RegistrationBuilder.CreateRegistration(RegistrationBuilder.ForDelegate(
                swt.ServiceType, (c, p) => exportMethod.Invoke(exports, null)));
        }

        private Type GetElementType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            return type;
        }
    }
}