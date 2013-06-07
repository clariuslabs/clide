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

namespace System.Diagnostics
{
    using System;
    using System.Windows;
    using Clide;
    using Clide.Diagnostics;
    using Clide.VisualStudio;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Provides tracing extensions on top of <see cref="ITracer"/>.
    /// </summary>
    public static class TracingExtensions
    {
        public static Action<Exception, string, string[]> ShowExceptionAction { get; set; }

        static TracingExtensions()
        {
            ShowExceptionAction = (exception, format, args) =>
            {
                GlobalServiceProvider.Instance.GetService<SVsUIShell, IVsUIShell>().ShowMessageBox(
                    string.Format(format, args), 
                    icon: MessageBoxImage.Error);
            };
        }

        /// <summary>
        /// Executes the given <paramref name="action"/> shielding any non-critical exceptions 
        /// and logging them to the <paramref name="tracer"/> with the given <paramref name="errorMessageFormat"/> message.
        /// </summary>
        [DebuggerStepThrough]
        public static Exception ShieldUI(this ITracer tracer, Action action, string errorMessageFormat, params string[] errorArgs)
        {
            Guard.NotNull(() => tracer, tracer);
            Guard.NotNull(() => action, action);
            Guard.NotNullOrEmpty(() => errorMessageFormat, errorMessageFormat);
            Guard.NotNull(() => errorArgs, errorArgs);

            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (ErrorHandler.IsCriticalException(ex))
                {
                    throw;
                }
                else
                {
                    tracer.Error(ex, errorMessageFormat, errorArgs);

                    ShowExceptionAction(ex, errorMessageFormat, errorArgs);

                    return ex;
                }
            }

            return null;
        }

        /// <summary>
        /// Executes the given <paramref name="action"/> shielding any non-critical exceptions 
        /// and logging them to the <paramref name="tracer"/> with the given <paramref name="errorMessage"/> message.
        /// </summary>
        [DebuggerStepThrough]
        public static Exception ShieldUI(this ITracer tracer, Action action, string errorMessage)
        {
            Guard.NotNullOrEmpty(() => errorMessage, errorMessage);

            return ShieldUI(tracer, action, errorMessage, new string[0]);
        }
    }
}
