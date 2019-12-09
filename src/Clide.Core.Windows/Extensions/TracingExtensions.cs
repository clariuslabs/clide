using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
namespace Clide
{

    /// <summary>
    /// Provides tracing extensions on top of <see cref="ITracer"/>.
    /// </summary>
    public static class TracingExtensions
    {
        private static AmbientSingleton<IErrorsManager> errorsManager = new AmbientSingleton<IErrorsManager>(new NullErrorsManager());
        private static AmbientSingleton<Action<Exception, string, string[]>> showException = new AmbientSingleton<Action<Exception, string, string[]>>(DefaultShowExceptionAction);

        private static Action<Exception, string, string[]> DefaultShowExceptionAction = (ex, format, args) =>
        {
            System.Windows.MessageBox.Show(
                ServiceProvider.Global.GetService<SVsUIShell, IVsUIShell>().GetMainWindow(),
                string.Format(format, args),
                "Visual Studio",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        };

        /// <summary>
        /// Executes the given <paramref name="action"/> shielding any non-critical exceptions
        /// and logging them to the <paramref name="tracer"/> with the given <paramref name="format"/> message.
        /// </summary>
        [DebuggerStepThrough]
        public static Exception ShieldUI(this ITracer tracer, Action action, string format, params string[] args)
        {
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
        public static Exception ShieldUI(this ITracer tracer, Action action, string errorMessage) =>
            ShieldUI(tracer, action, errorMessage, new string[0]);

        /// <summary>
        /// Traces an event of type <see cref="TraceEventType.Error"/> with the given exception and message and
        /// adds an error to the error list that will be handled by the provided function.
        /// </summary>
        /// <param name="tracer">The tracer that will perform the error logging.</param>
        /// <param name="exception">The exception that will be logged.</param>
        /// <param name="text">The message to log.</param>
        /// <param name="handler">A callback function to call when the user selects the error in the error list.
        /// The return value determines if the error has been fixed and can be deleted from the
        /// error list (<see langword="true"/>) or not.
        /// </param>
        /// <remarks>
        /// This overload adds an error to the error list, and allows a callback to run when the user double-clicks
        /// the error in the error list. The function return value determines if the error will be cleared from the
        /// list after the handler finishes running or not.
        /// </remarks>
        public static void Error(this ITracer tracer, Exception exception, string text, Func<bool> handler)
        {
            tracer.Error(exception, text);
            ErrorsManager.AddError(text, item =>
            {
                if (handler())
                    item.Remove();
            });
        }

        /// <summary>
        /// Traces an event of type <see cref="TraceEventType.Error"/> with the given message and
        /// adds an error to the error list that will be handled by the provided function.
        /// </summary>
        /// <param name="tracer">The tracer that will perform the error logging.</param>
        /// <param name="text">The message to log.</param>
        /// <param name="handler">A callback function to call when the user selects the error in the error list.
        /// The return value determines if the error has been fixed and can be deleted from the
        /// error list (<see langword="true"/>) or not.
        /// </param>
        /// <remarks>
        /// This overload adds an error to the error list, and allows a callback to run when the user double-clicks
        /// the error in the error list. The function return value determines if the error will be cleared from the
        /// list after the handler finishes running or not.
        /// </remarks>
        public static void Error(this ITracer tracer, string text, Func<bool> handler)
        {
            tracer.Error(text);
            ErrorsManager.AddError(text, item =>
            {
                if (handler())
                    item.Remove();
            });
        }

        /// <summary>
        /// Traces an event of type <see cref="TraceEventType.Warning"/> with the given exception and
        /// message and adds an error to the error list that will be handled by the provided function.
        /// </summary>
        /// <param name="tracer">The tracer that will perform the warning logging.</param>
        /// <param name="exception">The exception that will be logged.</param>
        /// <param name="text">The message to log.</param>
        /// <param name="handler">A callback function to call when the user selects the error in the error list.
        /// The return value determines if the error has been fixed and can be deleted from the
        /// error list (<see langword="true"/>) or not.
        /// </param>
        /// <remarks>
        /// This overload adds an error to the error list, and allows a callback to run when the user double-clicks
        /// the error in the error list. The function return value determines if the error will be cleared from the
        /// list after the handler finishes running or not.
        /// </remarks>
        public static void Warn(this ITracer tracer, Exception exception, string text, Func<bool> handler)
        {
            tracer.Warn(exception, text);
            ErrorsManager.AddWarning(text, item =>
            {
                if (handler())
                    item.Remove();
            });
        }

        /// <summary>
        /// Traces an event of type <see cref="TraceEventType.Warning"/> with the given message and
        /// adds an error to the error list that will be handled by the provided action.
        /// </summary>
        /// <param name="tracer">The tracer that will perform the warning logging.</param>
        /// <param name="text">The message to log.</param>
        /// <param name="handler">A callback function to call when the user selects the error in the error list.
        /// The return value determines if the error has been fixed and can be deleted from the
        /// error list (<see langword="true"/>) or not.
        /// </param>
        /// <remarks>
        /// This overload adds an error to the error list, and allows a callback to run when the user double-clicks
        /// the error in the error list. The function return value determines if the error will be cleared from the
        /// list after the handler finishes running or not.
        /// </remarks>
        public static void Warn(this ITracer tracer, string text, Func<bool> handler)
        {
            tracer.Warn(text);
            ErrorsManager.AddWarning(text, item =>
            {
                if (handler())
                    item.Remove();
            });
        }

        /// <summary>
        /// Gets or sets the errors manager to use to add errors to the error list.
        /// This is an ambient singleton, so  it is safe to replace it in multi-threaded test runs.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static IErrorsManager ErrorsManager
        {
            get { return errorsManager.Value; }
            set { errorsManager.Value = value; }
        }

        /// <summary>
        /// Gets or sets the action that is used to show error messages to
        /// the user. The signature has the exception being thrown, a
        /// message or format string, and optional formatting arguments.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Action<Exception, string, string[]> ShowExceptionAction
        {
            get { return showException.Value; }
            set { showException.Value = value; }
        }
    }
}