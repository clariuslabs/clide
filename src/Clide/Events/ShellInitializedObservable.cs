using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Reactive.Linq;
using Microsoft.VisualStudio.Threading;
using System.Reactive.Disposables;

namespace Clide.Events
{
    [Export(typeof(IObservable<ShellInitialized>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal partial class ShellInitializedObservable : IObservable<ShellInitialized>, IVsShellPropertyEvents
    {
        Lazy<IVsShell> shell;
        uint cookie;
        ShellInitialized data = new ShellInitialized();
        AsyncManualResetEvent initialized;
        IObservable<ShellInitialized> observable;

        [ImportingConstructor]
        public ShellInitializedObservable([Import(ContractNames.Interop.IVsShell)] Lazy<IVsShell> shell)
        {
            this.shell = shell;
            initialized = new AsyncManualResetEvent();

            object zombie;
            ErrorHandler.ThrowOnFailure(shell.Value.GetProperty((int)__VSSPROPID.VSSPROPID_Zombie, out zombie));

            var isZombie = (bool)zombie;
            observable = Observable.Create<ShellInitialized>(async o =>
            {
                if (isZombie)
                    await initialized.WaitAsync();

                o.OnNext(data);
                o.OnCompleted();

                return Disposable.Empty;
            });

            if (isZombie)
                ErrorHandler.ThrowOnFailure(shell.Value.AdviseShellPropertyChanges(this, out cookie));
        }

        public IDisposable Subscribe(IObserver<ShellInitialized> observer)
        {
            return observable.Subscribe(observer);
        }

        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object var)
        {
            if (propid == (int)__VSSPROPID.VSSPROPID_Zombie)
            {
                if ((bool)var == false)
                {
                    ErrorHandler.ThrowOnFailure(shell.Value.UnadviseShellPropertyChanges(cookie));
                    cookie = 0;
                    initialized.Set();
                }
            }

            return VSConstants.S_OK;
        }
    }
}