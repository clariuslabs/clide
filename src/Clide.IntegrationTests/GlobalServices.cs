using System;

namespace Clide
{
    public static class GlobalServices
    {
        public static IServiceProvider Instance => Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider;

        internal static T GetService<T>()
        {
            return Instance.GetService<T>();
        }

        internal static TService GetService<TRegistration, TService>()
        {
            return Instance.GetService<TRegistration, TService>();
        }
    }
}
