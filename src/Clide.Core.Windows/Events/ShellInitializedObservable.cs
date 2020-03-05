using System;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Clide.Events
{
    [Export(typeof(IObservable<ShellInitialized>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal partial class ShellInitializedObservable : IObservable<ShellInitialized>, IVsShellPropertyEvents
    {
        JoinableLazy<IVsShell> shell;
        uint cookie;
        ShellInitialized data = new ShellInitialized();
        CancellationTokenSource cts = new CancellationTokenSource();

        [ImportingConstructor]
        public ShellInitializedObservable(JoinableLazy<IVsShell> shell)
        {
            this.shell = shell;
            object zombie;
            ErrorHandler.ThrowOnFailure(shell.GetValue().GetProperty((int)__VSSPROPID.VSSPROPID_Zombie, out zombie));

            var isZombie = (bool)zombie;
            if (isZombie)
                ErrorHandler.ThrowOnFailure(shell.GetValue().AdviseShellPropertyChanges(this, out cookie));
            else
                cts.Cancel();
        }

        public IDisposable Subscribe(IObserver<ShellInitialized> observer)
        {
            if (cts.IsCancellationRequested)
            {
                observer.OnNext(data);
                observer.OnCompleted();
                return Disposable.Empty;
            }
            else
            {
                return cts.Token.Register(() =>
                {
                    observer.OnNext(data);
                    observer.OnCompleted();
                });
            }
        }

        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object var)
        {
            if (propid == (int)__VSSPROPID.VSSPROPID_Zombie)
            {
                if ((bool)var == false)
                {
                    ErrorHandler.ThrowOnFailure(shell.GetValue().UnadviseShellPropertyChanges(cookie));
                    cookie = 0;
                    cts.Cancel();
                }
            }

            return VSConstants.S_OK;
        }
    }
}
