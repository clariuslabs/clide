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

namespace Clide
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Clide.Properties;
    using Microsoft.VisualStudio.ComponentModelHost;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition;
    using Clide.Composition;
    using Microsoft.ComponentModel.Composition.Diagnostics;
    using System.IO;
    using System.Collections.Generic;

    internal class DevEnvLocator
    {
        private static readonly ITracer tracer = Tracer.Get<DevEnvLocator>();
        private static object syncRoot = new object();
        private CompositionContainer container;

        public IDevEnv Get(IServiceProvider services)
        {
            if (this.container == null)
            {
                lock (syncRoot)
                {
                    if (this.container != null)
                        return this.container.GetExportedValue<IDevEnv>();

                    InitializeContainer(services);
                }
            }

            return this.container.GetExportedValue<IDevEnv>();
        }

        private void InitializeContainer(IServiceProvider services)
        {
            using (tracer.StartActivity(Strings.DevEnvFactory.CreatingComposition))
            {
                var composition = services.GetService<SComponentModel, IComponentModel>();

                // It would be user mistake to add Clide to MEF, but we have to account for it 
                // anyway.
                var catalog = new AggregateCatalog(
                    new AssemblyCatalog(typeof(IDevEnv).Assembly),
                    // We always expose our own composition service and export provider.
                    SingletonCatalog.Create<ICompositionService>(ContractNames.ICompositionService, new Lazy<ICompositionService>(() => container)));

                container = new CompositionContainer(catalog, composition.DefaultExportProvider);

                var info = new CompositionInfo(catalog, container);
                var rejected = info.PartDefinitions.Where(part => part.IsPrimaryRejection).ToList();
                if (rejected.Count > 0)
                {
                    tracer.Error(Strings.DevEnvFactory.CompositionErrors(rejected.Count));
                    var writer = new StringWriter();
                    rejected.ForEach(part => PartDefinitionInfoTextFormatter.Write(part, writer));
                    tracer.Error(writer.ToString());
                    throw new InvalidOperationException(
                        Strings.DevEnvFactory.CompositionErrors(rejected.Count) + Environment.NewLine +
                        writer.ToString());
                }

#if DEBUG
                // Log information about the composition container in debug mode.
                {
                    var infoWriter = new StringWriter();
                    CompositionInfoTextFormatter.Write(info, infoWriter);
                    tracer.Info(infoWriter.ToString());
                }
#else
                if (Debugger.IsAttached)
                {
                    // Log information about the composition container when debugger is attached too.
                    var infoWriter = new StringWriter();
                    CompositionInfoTextFormatter.Write(info, infoWriter);
                    tracer.Info(infoWriter.ToString());
                }
#endif
            }
        }
    }
}
