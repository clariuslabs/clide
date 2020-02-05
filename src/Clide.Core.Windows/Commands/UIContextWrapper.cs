using System;
using Microsoft.VisualStudio.Shell;

namespace Clide.Commands
{
    /// <summary>
    /// Wrapper for <see cref="Microsoft.VisualStudio.Shell.UIContext"/> in order to be used in unit tests
    /// </summary>
    class UIContextWrapper
    {
        readonly bool isActive;
        readonly UIContext vsUIContext;

        public UIContextWrapper(UIContext vsUIContext)
        {
            this.vsUIContext = vsUIContext;
        }

        public UIContextWrapper(bool isActive)
        {
            this.isActive = isActive;
        }

        public bool IsActive => vsUIContext?.IsActive ?? isActive;
    }
}
