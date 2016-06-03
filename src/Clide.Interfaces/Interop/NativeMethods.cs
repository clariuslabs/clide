using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Clide.Interop
{
	static class NativeMethods
	{
		[DllImport ("ole32.dll")]
		internal static extern int CreateBindCtx (int reserved, out IBindCtx ppbc);
		[DllImport ("ole32.dll")]
		internal static extern int GetRunningObjectTable (int reserved, out IRunningObjectTable prot);

		internal static bool Succeeded (int hr)
		{
			return hr >= 0;
		}

		internal static bool Failed (int hr)
		{
			return hr < 0;
		}
	}
}