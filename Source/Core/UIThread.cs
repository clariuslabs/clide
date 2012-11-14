using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;

namespace Clide
{
    /// <summary>
    /// Default UI thread invoker implementation.
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
	[Export(typeof(IUIThread))]
	internal class UIThread : IUIThread
	{
        public void Invoke(Action action)
        {
            ThreadHelper.Generic.Invoke(action);
        }

        public TResult Invoke<TResult>(Func<TResult> function)
        {
            return ThreadHelper.Generic.Invoke(function);
        }
    }
}
