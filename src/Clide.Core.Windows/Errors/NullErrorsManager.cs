using System;
namespace Clide
{

    class NullErrorsManager : IErrorsManager
    {
        public IErrorItem AddError(string message, Action<IErrorItem> handler)
        {
            return NullErrorItem.Instance;
        }

        public IErrorItem AddWarning(string text, Action<IErrorItem> handler)
        {
            return NullErrorItem.Instance;
        }

        public void ShowErrors()
        {
        }

        public void ClearErrors()
        {
        }

        private class NullErrorItem : IErrorItem
        {
            public static IErrorItem Instance = new NullErrorItem();

            public void Remove()
            {
            }
        }
    }
}