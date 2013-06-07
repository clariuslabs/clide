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
    using System;
    using System.ComponentModel.Composition;
    using System.Windows;
    using System.Windows.Threading;
    using Clide.Composition;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Default UI thread invoker implementation, which uses the 
    /// current <see cref="Dispatcher.CurrentDispatcher"/> by default 
    /// or the one initialized from the host.
    /// </summary>
	[Component(typeof(IUIThread))]
	internal class UIThread : IUIThread
	{
        private static Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        private static readonly IUIThread uiThread = new UIThread();

        internal static IUIThread Default { get { return uiThread; } }

        public static void Initialize(Dispatcher dispatcher)
        {
            UIThread.dispatcher = dispatcher;
        }

        public TResult Invoke<TResult>(Func<TResult> function)
        {
            if (dispatcher.CheckAccess())
                return function();
            else
                return dispatcher.Invoke<TResult>(function);
        }

        public void Invoke(Action action)
        {
            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(action);
        }

        public void BeginInvoke(Action action)
        {
            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.BeginInvoke(action);
        }
    }
}
