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

namespace Clide.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio;
    using Clide.Composition;

    [Component(typeof(IGlobalEvents), typeof(IShellEvents))]
    internal class ShellEvents : IDisposable, IVsShellPropertyEvents, IShellEvents
    {
        private IServiceProvider services;
        private IVsShell shellService;
        private uint shellCookie;
        private event EventHandler initialized = (sender, args) => { };

        public ShellEvents(IServiceProvider serviceProvider)
        {
            Guard.NotNull(() => serviceProvider, serviceProvider);

            this.services = serviceProvider;
            this.shellService = serviceProvider.GetService<SVsShell, IVsShell>();

            object isZombie;
            ErrorHandler.ThrowOnFailure(this.shellService.GetProperty((int)__VSSPROPID.VSSPROPID_Zombie, out isZombie));

            this.IsInitialized = !((bool)isZombie);

            ErrorHandler.ThrowOnFailure(
                this.shellService.AdviseShellPropertyChanges(this, out this.shellCookie));
        }

        public void Dispose()
        {
            try
            {
                if (this.shellCookie != 0)
                    ErrorHandler.ThrowOnFailure(this.shellService.UnadviseShellPropertyChanges(this.shellCookie));
            }
            catch (Exception ex)
            {
                if (ErrorHandler.IsCriticalException(ex))
                    throw;
            }

            this.shellCookie = 0;
        }

        public bool IsInitialized { get; private set; }

        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object var)
        {
            if (propid == (int)__VSSPROPID.VSSPROPID_Zombie)
            {
                if ((bool)var == false)
                {
                    ErrorHandler.ThrowOnFailure(this.shellService.UnadviseShellPropertyChanges(this.shellCookie));
                    this.shellCookie = 0;

                    this.IsInitialized = true;
                    // Raise the events for handlers that have been subscribed before this point.
                    this.initialized(this, EventArgs.Empty);
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Occurs when the shell has finished initializing.
        /// </summary>
        public event EventHandler Initialized
        {
            add
            {
                // It we have already been initialized, invoke the handler right-away, 
                // there's no need to keep the handler subscribed passed this point.
                if (this.IsInitialized)
                    value(this, EventArgs.Empty);
                else
                    this.initialized += value;
            }
            remove { this.initialized -= value; }
        }
    }
}
