using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Clide.Interop
{
    static class NativeMethods
    {
        public static readonly Guid IID_IServiceProvider = typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider).GUID;
        public static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        [DllImport("ole32.dll")]
        internal static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);
        [DllImport("ole32.dll")]
        internal static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        internal static bool Succeeded(int hr)
        {
            return hr >= 0;
        }

        internal static bool Failed(int hr)
        {
            return hr < 0;
        }
    }
}
