using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Microsoft.VisualStudio.Shell.Interop
{

    [ComImport, Guid("BB0D419C-04CD-457A-BC3C-954F447EC806"), TypeIdentifier, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IVsSharedProjectReferencesHelper
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        bool ChangeSharedMSBuildFileImports(
            [In, MarshalAs(UnmanagedType.Interface)] IVsHierarchy importingProject,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] Array importFullPathsToRemove,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] Array importFullPathsToAdd,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szImportLabel);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        bool ChangeSharedProjectReferences(
            [In, MarshalAs(UnmanagedType.Interface)] IVsHierarchy ReferencingProject,
            [In] int cReferencesToRemove,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 1)] object[] referencesToRemove,
            [In] int cReferencesToAdd,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 3)] object[] referencesToAdd);
    }
}
