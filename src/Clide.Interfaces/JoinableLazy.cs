using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace Clide
{
    public class JoinableLazy<T>
    {
        static JoinableTaskContext sharedContext;
        readonly JoinableTaskFactory taskFactory;
        readonly AsyncLazy<T> asyncLazy;

        public JoinableLazy(Func<Task<T>> valueFactory, bool executeOnMainThread = false)
            : this(valueFactory, ThreadHelper.JoinableTaskFactory, executeOnMainThread)
        {
        }

        public JoinableLazy(Func<Task<T>> valueFactory, JoinableTaskFactory taskFactory, bool executeOnMainThread = false)
        {
            this.taskFactory = taskFactory;
            asyncLazy = new AsyncLazy<T>(async () => {
                if (executeOnMainThread)
                    await GetTaskFactory().SwitchToMainThreadAsync();

                return await valueFactory();
            }, GetTaskFactory());
        }

        public JoinableLazy(Func<T> valueFactory, bool executeOnMainThread = false)
          : this(valueFactory, ThreadHelper.JoinableTaskFactory, executeOnMainThread)
        {
        }

        public JoinableLazy(Func<T> valueFactory, JoinableTaskFactory taskFactory, bool executeOnMainThread = false)
        {
            this.taskFactory = taskFactory;
            asyncLazy = new AsyncLazy<T>(async () => {
                if (executeOnMainThread)
                    await GetTaskFactory().SwitchToMainThreadAsync();

                return valueFactory();
            }, GetTaskFactory());
        }


        public static void SetContext (JoinableTaskContext context) => sharedContext = context;

        public T GetValue() => GetTaskFactory().Run(async () => await asyncLazy.GetValueAsync());

        public Task<T> GetValueAsync() => GetValueAsync(CancellationToken.None);

        public Task<T> GetValueAsync(CancellationToken cancellationToken) => asyncLazy.GetValueAsync(cancellationToken);

        JoinableTaskFactory GetTaskFactory() => sharedContext?.Factory ?? taskFactory;
    }
}
