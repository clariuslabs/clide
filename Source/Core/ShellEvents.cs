using Microsoft.VisualStudio.Shell;
namespace Clide
{
    using System;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [Export(typeof(IShellEvents))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class ShellEvents : IDisposable, IVsShellPropertyEvents, IShellEvents
    {
        private IServiceProvider services;
        private IVsShell shellService;
        private uint shellCookie;
        private event EventHandler initialized = (sender, args) => { };

        [ImportingConstructor]
        public ShellEvents([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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
