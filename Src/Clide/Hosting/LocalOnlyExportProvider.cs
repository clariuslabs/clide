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

namespace Clide
{
    using Clide.Properties;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    // Alias to the constructor we use to make up a new contract definition with specific 
    // metadata. This is needed because the reference assemblies don't expose this constructor 
    // that does exist in .NET 4.0.
    using ImportDefinitionConstructor = System.Func<
        string,
        string,
        System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Type>>,
        System.ComponentModel.Composition.Primitives.ImportCardinality,
        bool, bool,
        System.ComponentModel.Composition.CreationPolicy,
        System.Collections.Generic.IDictionary<string, object>, System.ComponentModel.Composition.Primitives.ContractBasedImportDefinition>;


    /// <summary>
    /// Avoids exposing to VS the Clide APIs, which are only for the specific 
    /// hosting package and its provided interfaces. Also prevents re-exposing 
    /// VS global catalog exports which would cause duplicates.
    /// </summary>
    internal class LocalOnlyExportProvider : ExportProvider
    {
        private static readonly ITracer tracer = Tracer.Get<LocalOnlyExportProvider>();
        private static readonly ImportDefinitionConstructor ImportDefinitionFactory;

        private ExportProvider innerProvider;

        static LocalOnlyExportProvider()
        {
            var args = typeof(ImportDefinitionConstructor)
                .GetMethod("Invoke")
                .GetParameters();

            var ctor = typeof(ContractBasedImportDefinition)
                .GetConstructor(args
                    .Select(p => p.ParameterType)
                    .ToArray());

            if (ctor != null)
            {
                // Changing the metadata of the import, or even create a new 
                // contract-based import with specific metadata is not 
                // exposed via API, even though it exists in the .NET 4.0 
                // GAC assembly as a public constructor overload. The 
                // corresponding Reference Assembly, however, hides it, 
                // so we need to resort to invoking the actual type constructor
                // via reflection, which we make more performant by 
                // compiling a lambda.
                var parameters = args
                    .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                    .ToArray();

                ImportDefinitionFactory = Expression
                    .Lambda<ImportDefinitionConstructor>(Expression.New(ctor, parameters), parameters)
                    .Compile();
            }
            else
            {
                tracer.Error(Strings.Hosting.UnsupportedRuntime);
                throw new NotSupportedException(Strings.Hosting.UnsupportedRuntime);
            }
        }

        public LocalOnlyExportProvider(ExportProvider innerProvider)
        {
            this.innerProvider = innerProvider;
        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            IEnumerable<Export> exports;

            var importSource = typeof(ImportAttribute).Assembly.GetType("System.ComponentModel.Composition.ImportSource");
            if (importSource == null)
            {
                tracer.Error(Strings.Hosting.UnsupportedRuntime);
                throw new NotSupportedException(Strings.Hosting.UnsupportedRuntime);
            }

            // This special kind of metadata is checked y the base ExportProvider 
            // to determine whether to query for exports from the parent container, 
            // which in our case is the global VS composition container.
            // We want to avoid querying VS composition container as this 
            // would cause a stack overflow, as we're answering that call 
            // here ourselves (VSMEF rewrote the GetExportsCore so that it 
            // queries all created VsCompositionContainers for their exports.
            var metadata = new Dictionary<string, object>
                {
                    {  importSource.FullName, Enum.Parse(importSource, "Local") }
                };

            var contractDefinition = definition as ContractBasedImportDefinition;
            if (contractDefinition == null)
            {
                // This provider does not support non-contract based exports.
                return Enumerable.Empty<Export>();
            }

            var localImport = ImportDefinitionFactory.Invoke(
                contractDefinition.ContractName,
                contractDefinition.RequiredTypeIdentity,
                contractDefinition.RequiredMetadata,
                contractDefinition.Cardinality,
                contractDefinition.IsRecomposable,
                contractDefinition.IsPrerequisite,
                contractDefinition.RequiredCreationPolicy,
                metadata);

            this.innerProvider.TryGetExports(localImport, atomicComposition, out exports);

            return exports.Where(e => !IsClideExport(e));
        }

        private static bool IsClideExport(Export e)
        {
            return
                e.Metadata.ContainsKey("ExportTypeIdentity") &&
                e.Metadata["ExportTypeIdentity"].ToString().StartsWith("Clide");
        }
    }
}
