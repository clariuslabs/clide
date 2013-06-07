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

            // Unfortunately, VSMEF rewrote the logic of how exports are retrieved 
            // and the catalog doesn't have everything that can be retrieved, so we 
            // have to resort to actually invoking the GetExportedValueOrDefault 
            // to really know if the export is there or not :(
            var contractName = AttributedModelServices.GetContractName(swt.ServiceType);
            if (!catalog.Parts.SelectMany(part => part.ExportDefinitions).Any(e => e.ContractName.Equals(contractName)) && 
                getExport.MakeGenericMethod(swt.ServiceType).Invoke(exports, null) == null)
                yield break;

            yield return RegistrationBuilder.CreateRegistration(RegistrationBuilder.ForDelegate(
                swt.ServiceType, (c, p) => getExport.MakeGenericMethod(swt.ServiceType).Invoke(exports, null)));
        }
    }
}