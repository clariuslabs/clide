using System;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Shell
{
    [Guid("1EAA526A-0898-11d3-B868-00C04F79F802"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface SVsAppId
    {
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1EAA526A-0898-11d3-B868-00C04F79F802")]
    internal interface IVsAppId
    {
        [PreserveSig]
        int SetSite(OLE.Interop.IServiceProvider pSP);
        [PreserveSig]
        int GetProperty(int propid, [MarshalAs(UnmanagedType.Struct)] out object pvar);
        [PreserveSig]
        int SetProperty(int propid, [MarshalAs(UnmanagedType.Struct)] object var);
        [PreserveSig]
        int GetGuidProperty(int propid, out Guid guid);
        [PreserveSig]
        int SetGuidProperty(int propid, ref Guid rguid);
        [PreserveSig]
        int Initialize();
    }

    internal enum VSAPropID
    {
        VSAPROPID_IsolationInstallationName = -8627,
        VSAPROPID_IsolationInstallationId = -8628,
        VSAPROPID_IsolationInstallationVersion = -8629,
        VSAPROPID_IsolationInstallationWorkloads = -8630,
        VSAPROPID_IsolationInstallationPackages = -8631,
        VSAPROPID_ChannelId = -8638,
        VSAPROPID_ProductDisplayVersion = -8641,
        VSAPROPID_ChannelTitle = -8643,
        VSAPROPID_SKUName = -8648,
    }

    internal static class IVsAppIdExtensions
    {
        internal static T GetProperty<T>(this IVsAppId vsAppId, VSAPropID propid)
        {
            if (vsAppId != null && ErrorHandler.Succeeded(vsAppId.GetProperty((int)propid, out var prop)))
            {
                return (T)prop;
            }
            else
            {
                return default(T);
            }
        }
    }
}
