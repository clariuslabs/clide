using Microsoft.VisualStudio.Shell;
namespace Clide
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Provides tracing extensions on top of <see cref="ITracer"/>.
    /// </summary>
    public static class TracingExtensions
    {
        internal static Action<Exception, string, string[]> ShowExceptionAction = (ex, format, args) =>
        {
            System.Windows.MessageBox.Show(ServiceProvider.GlobalProvider
                .GetService<SVsUIShell, IVsUIShell>().GetMainWindow(), 
                string.Format(format, args),
                "Visual Studio", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        };

        /// <summary>
        /// Executes the given <paramref name="action"/> shielding any non-critical exceptions 
        /// and logging them to the <paramref name="tracer"/> with the given <paramref name="format"/> message.
        /// </summary>
        [DebuggerStepThrough]
        public static Exception ShieldUI(this ITracer tracer, Action action, string format, params string[] args)
        {
            Guard.NotNull(() => tracer, tracer);
            Guard.NotNull(() => action, action);
            Guard.NotNullOrEmpty(() => format, format);
            Guard.NotNull(() => args, args);

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
                    tracer.Error(ex, format, args);

                    ShowExceptionAction(ex, format, args);

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
