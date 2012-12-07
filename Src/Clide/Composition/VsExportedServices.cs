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

namespace Clide.Composition
{
    using System;
    using System.ComponentModel.Composition;
    using EnvDTE;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Provides core VS services as MEF exports, with our own custom <see cref="VsContractNames"/> to 
    /// avoid potential collisions in the future.
    /// </summary>
    /// <devdoc>
    /// It's a best practice in VS-MEF world to not re-expose services you don't own without a custom 
    /// contract name. This allows the service owner to provide the service as a MEF export in the 
    /// future, without risking to break your package.
    /// </devdoc>
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class VsExportedServices
    {
        private IServiceProvider serviceProvider;

        [ImportingConstructor]
        public VsExportedServices([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Export(VsContractNames.IComponentModel)]
        public IComponentModel ComponentModel
        {
            get { return this.serviceProvider.GetService<SComponentModel, IComponentModel>(); }
        }

        [Export(VsContractNames.IVsUIShell)]
        public IVsUIShell UIShell
        {
            get { return this.serviceProvider.GetService<SVsUIShell, IVsUIShell>(); }
        }

        [Export(VsContractNames.IVsShell)]
        public IVsShell VsShell
        {
            get { return this.serviceProvider.GetService<SVsShell, IVsShell>(); }
        }

        [Export(VsContractNames.DTE)]
        public DTE DTE
        {
            get { return this.serviceProvider.GetService<SDTE, DTE>(); }
        }
    }
}
