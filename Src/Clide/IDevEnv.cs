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
    using Clide.Events;
    using Microsoft.Practices.ServiceLocation;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Entry point interface for the environment components.
    /// </summary>
    public interface IDevEnv : IFluentInterface
    {
        /// <summary>
        /// Gets a value indicating whether the shell has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets a value indicating whether the environment is running with 
        /// elevated permissions.
        /// </summary>
        bool IsElevated { get; }

        /// <summary>
        /// Occurs when the shell has finished initializing.
        /// </summary>
        event EventHandler Initialized;

        /// <summary>
        /// Gets the dialog window factory.
        /// </summary>
        IDialogWindowFactory DialogWindowFactory { get; }

        /// <summary>
        /// Gets the errors manager
        /// </summary>
        IErrorsManager Errors { get; }

        /// <summary>
        /// Gets the message box service.
        /// </summary>
        IMessageBoxService MessageBoxService { get; }

        /// <summary>
        /// Gets the output window manager.
        /// </summary>
        IOutputWindowManager OutputWindow { get; }

        /// <summary>
        /// Gets the service locator.
        /// </summary>
        IServiceLocator ServiceLocator { get; }

        /// <summary>
        /// Gets the status bar.
        /// </summary>
        IStatusBar StatusBar { get; }

        /// <summary>
        /// Gets the tool windows.
        /// </summary>
        IEnumerable<IToolWindow> ToolWindows { get; }

        /// <summary>
        /// Gets the UI thread.
        /// </summary>
        IUIThread UIThread { get; }

        /// <summary>
        /// Gets the reference service.
        /// </summary>
        IReferenceService ReferenceService { get; }

        /// <summary>
        /// Exits Visual Studio.
        /// </summary>
        /// <param name="saveAll">Whether to save all pending changes before exiting.</param>
        void Exit(bool saveAll = true);

        /// <summary>
        /// Restarts Visual Studio.
        /// </summary>
        /// <param name="saveAll">Whether to save all pending changes before exiting.</param>
        /// <returns><see langword="true"/> if the operation succeeded; <see langword="false"/> otherwise.</returns>
        bool Restart(bool saveAll = true);
    }
}