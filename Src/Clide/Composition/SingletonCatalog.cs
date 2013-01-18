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

    internal static class SingletonCatalog
    {
        public static SingletonCatalog<T> Create<T>(Lazy<T> value)
        {
            return new SingletonCatalog<T>(AttributedModelServices.GetContractName(typeof(T)), value);
        }

        public static SingletonCatalog<T> Create<T>(string contractName, Lazy<T> value)
        {
            return new SingletonCatalog<T>(contractName, value);
        }
    }

    internal class SingletonCatalog<T> : ComposablePartCatalog
    {
        private IQueryable<ComposablePartDefinition> parts;

        public SingletonCatalog(string contractName, Lazy<T> value)
        {
            this.parts = new[] { new SingletonPartDefinition(contractName, value) }.AsQueryable();
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return this.parts; }
        }

        private class SingletonPartDefinition : ComposablePartDefinition
        {
            private SingletonPart part;

            public SingletonPartDefinition(string contractName, Lazy<T> value)
            {
                this.part = new SingletonPart(contractName, value);
            }

            public override ComposablePart CreatePart()
            {
                return this.part;
            }

            public override IEnumerable<ExportDefinition> ExportDefinitions
            {
                get { return this.part.ExportDefinitions; }
            }

            public override IEnumerable<ImportDefinition> ImportDefinitions
            {
                get { return this.part.ImportDefinitions; }
            }
        }

        private class SingletonPart : ComposablePart
        {
            private Lazy<T> value;
            private ExportDefinition export;

            public SingletonPart(string contractName, Lazy<T> value)
            {
                this.value = value;
                this.export = new ExportDefinition(
                    contractName,
                    new Dictionary<string, object>()
                   {
                       { 
                           CompositionConstants.ExportTypeIdentityMetadataName, 
                           AttributedModelServices.GetTypeIdentity(typeof(T)) 
                       }, 
                       {
                           LocalDecoratingCatalog.IsLocalKey, 
                           true
                       }
                   });
            }

            public override IEnumerable<ExportDefinition> ExportDefinitions
            {
                get { return new[] { export }; }
            }

            public override object GetExportedValue(ExportDefinition definition)
            {
                return this.value.Value;
            }

            public override IEnumerable<ImportDefinition> ImportDefinitions
            {
                get { return Enumerable.Empty<ImportDefinition>(); }
            }

            public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
            {
            }
        }
    }
}
