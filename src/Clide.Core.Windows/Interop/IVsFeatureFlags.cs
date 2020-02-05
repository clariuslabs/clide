using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Internal.VisualStudio.Shell.Interop
{
    [ComImport, Guid("AD44B8B9-B646-4B18-8847-150695AEC480"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), TypeIdentifier]
    public interface IVsFeatureFlags
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        bool IsFeatureEnabled([In, MarshalAs(UnmanagedType.LPWStr)] string featureName, [In] bool defaultValue);
    }
}
