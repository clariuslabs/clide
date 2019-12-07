using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace Clide
{
    public static class JoinableLazy
    {
        public static JoinableLazy<T> Create<T>(Func<T> valueFactory, JoinableTaskFactory taskFactory, bool executeOnMainThread = false)
            => new JoinableLazy<T>(valueFactory, taskFactory, executeOnMainThread);
    }

    /// <summary>
    /// Provides thread-safe support for lazy initialization, with options for sync and async value factories
    /// </summary>
    /// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
    public class JoinableLazy<T>
    {
        static JoinableTaskFactory defaultTaskFactory;
        readonly JoinableTaskFactory taskFactory;
        readonly AsyncLazy<T> asyncLazy;

        static JoinableLazy()
        {
            //We need to do this since accesing ThreadHelper from outside devenv.exe process will throw
            try
            {
                // HACK for windows
                defaultTaskFactory = Type
                    .GetType("Microsoft.VisualStudio.Shell.ThreadHelper, Microsoft.VisualStudio.Shell")
                    ?.GetProperty("JoinableTaskFactory", System.Reflection.BindingFlags.Static)
                    ?.GetValue(null) as JoinableTaskFactory;
            }
            catch { }
        }

        /// <summary>
        /// Gets a value indicating whether the value factory has been invoked
        /// </summary>
        public bool IsValueCreated => asyncLazy.IsValueCreated;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinableLazy{T}"/> class. 
        /// A default TaskFactory will be used to execute the lazy initialization. 
        /// The default value is <see cref="ThreadHelper.JoinableTaskFactory"/>, however it can be overriden 
        /// by using the <see cref="JoinableLazy{T}.SetTaskFactory(JoinableTaskFactory)"/> static method.
        /// Note that <see cref="ThreadHelper.JoinableTaskFactory"/> will only be accesible from inside devenv.exe process
        /// </summary>
        /// <param name="asyncValueFactory">An async delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        /// <param name="executeOnMainThread">A value that indicates if switching to the main thread is needed in order to execute the async value factory.</param>
        /// <exception cref="ArgumentNullException">Throws if the default TaskFactory is null</exception>
        public JoinableLazy(Func<Task<T>> asyncValueFactory, bool executeOnMainThread = false)
            : this(asyncValueFactory, defaultTaskFactory, executeOnMainThread)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinableLazy{T}"/> class. 
        /// </summary>
        /// <param name="asyncValueFactory">An async delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        /// <param name="taskFactory">The factory to use when invoking the async value factory in <see cref="JoinableLazy{T}.GetValue"/> 
        /// or <see cref="JoinableLazy{T}.GetValueAsync"/> to avoid deadlocks when the main thread is required.</param>
        /// <param name="executeOnMainThread">A value that indicates if switching to the main thread is needed in order to execute the async value factory.</param>
        /// <exception cref="ArgumentNullException">Throws if taskFactory is null</exception>
        public JoinableLazy(Func<Task<T>> asyncValueFactory, JoinableTaskFactory taskFactory, bool executeOnMainThread = false)
        {
            this.taskFactory = taskFactory ?? throw new ArgumentNullException(nameof(taskFactory));
            asyncLazy = new AsyncLazy<T>(async () =>
            {
                if (executeOnMainThread)
                    await taskFactory.SwitchToMainThreadAsync();

                return await asyncValueFactory();
            }, taskFactory);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinableLazy{T}"/> class. 
        /// A default TaskFactory will be used to execute the lazy initialization. 
        /// The default value is <see cref="ThreadHelper.JoinableTaskFactory"/>, however it can be overriden 
        /// by using the <see cref="JoinableLazy{T}.SetTaskFactory(JoinableTaskFactory)"/> static method.
        /// Note that <see cref="ThreadHelper.JoinableTaskFactory"/> will only be accesible from inside devenv.exe process
        /// </summary>
        /// <param name="valueFactory">A delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        /// <param name="executeOnMainThread">A value that indicates if switching to the main thread is needed in order to execute the value factory.</param>
        public JoinableLazy(Func<T> valueFactory, bool executeOnMainThread = false)
          : this(valueFactory, defaultTaskFactory, executeOnMainThread)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinableLazy{T}"/> class. 
        /// </summary>
        /// <param name="valueFactory">A delegate that is invoked to produce the lazily initialized value when it is needed.</param>
        /// <param name="taskFactory">The factory to use when invoking the value factory in <see cref="JoinableLazy{T}.GetValue"/> 
        /// or <see cref="JoinableLazy{T}.GetValueAsync"/> to avoid deadlocks when the main thread is required.</param>
        /// <param name="executeOnMainThread">A value that indicates if switching to the main thread is needed in order to execute the value factory.</param>
        /// <exception cref="ArgumentNullException">Throws if taskFactory is null</exception>
        public JoinableLazy(Func<T> valueFactory, JoinableTaskFactory taskFactory, bool executeOnMainThread = false)
        {
            this.taskFactory = taskFactory ?? throw new ArgumentNullException(nameof(taskFactory));
            asyncLazy = new AsyncLazy<T>(async () =>
            {
                if (executeOnMainThread)
                    await taskFactory.SwitchToMainThreadAsync();

                return valueFactory();
            }, taskFactory);
        }

        /// <summary>
        /// Sets the default factory to use when invoking the value factory in <see cref="JoinableLazy{T}.GetValue"/> 
        /// or <see cref="JoinableLazy{T}.GetValueAsync"/> to avoid deadlocks when the main thread is required.
        /// </summary>
        /// <param name="taskFactory">The TaskFactory to set as the default value to use.</param>
        public static void SetTaskFactory(JoinableTaskFactory taskFactory)
            => defaultTaskFactory = taskFactory ?? throw new ArgumentNullException(nameof(taskFactory));

        /// <summary>
        ///  Gets the lazily initialized value of the current <see cref="JoinableLazy{T}"/> instance.
        /// </summary>
        /// <returns>The lazily initialized value of the current <see cref="JoinableLazy{T}"/> instance.</returns>
        public T GetValue() => taskFactory.Run(async () => await asyncLazy.GetValueAsync());

        /// <summary>
        /// Gets the task that produces or has produced the value of the current <see cref="JoinableLazy{T}"/> instance.
        /// </summary>
        /// <returns>A task whose result is the lazily initialized value of the current <see cref="JoinableLazy{T}"/> instance.</returns>
        public Task<T> GetValueAsync() => GetValueAsync(CancellationToken.None);

        /// <summary>
        /// Gets the task that produces or has produced the value of the current <see cref="JoinableLazy{T}"/> instance.
        /// </summary>
        /// <param name="cancellationToken">A token whose cancellation indicates that the caller is no longer interested
        //  in the result. Note that this will not cancel the value factory (since other callers may exist), 
        // /but this token will result in the cancellation of  the returned Task.
        /// <returns>A task whose result is the lazily initialized value of the current <see cref="JoinableLazy{T}"/> instance.</returns>
        public Task<T> GetValueAsync(CancellationToken cancellationToken) => asyncLazy.GetValueAsync(cancellationToken);
    }
}
