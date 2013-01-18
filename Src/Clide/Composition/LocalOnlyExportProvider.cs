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
    using Clide.Properties;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Avoids exposing to VS the Clide APIs, which are only for the specific 
    /// hosting package and its provided interfaces. Also prevents re-exposing 
    /// VS global catalog exports which would cause duplicates.
    /// </summary>
    internal class LocalOnlyExportProvider : ExportProvider
    {
        private ThreadLocal<bool> gettingExports = new ThreadLocal<bool>(() => false);
        private static readonly ITracer tracer = Tracer.Get<LocalOnlyExportProvider>();

        private ExportProvider innerProvider;

        public LocalOnlyExportProvider(ExportProvider innerProvider)
        {
            this.innerProvider = innerProvider;
        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            if (gettingExports.Value)
                return Enumerable.Empty<Export>();

            try
            {
                gettingExports.Value = true;

                IEnumerable<Export> exports;

                var contractDefinition = definition as ContractBasedImportDefinition;
                if (contractDefinition == null)
                {
                    // This provider does not support non-contract based exports.
                    return Enumerable.Empty<Export>();
                }

                this.innerProvider.TryGetExports(contractDefinition, atomicComposition, out exports);

                exports = exports.ToList();

                return exports.Where(e => IsLocal(e) && !IsClideExport(e));

            }
            finally
            {
                gettingExports.Value = false;
            }
        }

        private static bool IsLocal(Export e)
        {
            return e.Metadata.ContainsKey(LocalDecoratingCatalog.IsLocalKey) && 
                (bool)e.Metadata[LocalDecoratingCatalog.IsLocalKey];
        }

        private static bool IsClideExport(Export e)
        {
            return
                e.Metadata.ContainsKey("ExportTypeIdentity") &&
                e.Metadata["ExportTypeIdentity"].ToString().StartsWith("Clide");
        }
    }
}
