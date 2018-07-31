using Microsoft.VisualStudio.Shell;
namespace Clide
{

    class ErrorItem : IErrorItem
    {
        readonly ErrorListProvider provider;
        readonly ErrorTask task;

        public ErrorItem(ErrorListProvider provider, ErrorTask task)
        {
            this.provider = provider;
            this.task = task;
        }

        public void Remove()
        {
            if (provider.Tasks.Contains(task))
                provider.Tasks.Remove(task);
        }
    }
}
