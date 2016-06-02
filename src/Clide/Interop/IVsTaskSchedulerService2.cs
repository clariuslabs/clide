using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Shell.Interop
{
	/// <summary>
	/// Internal.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[ComImport, Guid ("8176DC77-36E2-4987-955B-9F63C6F3F229"), InterfaceType (ComInterfaceType.InterfaceIsIUnknown), TypeIdentifier]
	public interface IVsTaskSchedulerService2
	{
		/// <summary>
		/// Internal.
		/// </summary>
		[return: MarshalAs (UnmanagedType.IUnknown)]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		object GetAsyncTaskContext ();
	}
}
