using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio;

namespace Clide.Interop
{
	static class RunningObjects
	{
		static readonly Regex versionExpr = new Regex (@"Microsoft Visual Studio (?<version>\d\d\.\d)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		public static EnvDTE.DTE GetDTE(TimeSpan retryTimeout)
		{
			var processId = Process.GetCurrentProcess ().Id;
			var devEnv = Process.GetCurrentProcess ().MainModule.FileName;

			if (Path.GetFileName (devEnv) != "devenv.exe")
				throw new NotSupportedException ("Can only retrieve the current DTE from a running devenv.exe instance.");

			// C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe
			var version = versionExpr.Match (devEnv).Groups["version"];
			if (!version.Success)
				throw new NotSupportedException ("Could not determine Visual Studio version from running process from " + devEnv);

			return GetComObject<EnvDTE.DTE> (string.Format ("!{0}.{1}:{2}",
				"VisualStudio.DTE", version.Value, processId), retryTimeout);
		}

		public static EnvDTE.DTE GetDTE(string visualStudioVersion, int processId, TimeSpan retryTimeout)
		{
			return GetComObject<EnvDTE.DTE> (string.Format ("!{0}.{1}:{2}",
				"VisualStudio.DTE", visualStudioVersion, processId), retryTimeout);
		}

		public static T GetComObject<T> (string monikerName, TimeSpan retryTimeout)
		{
			object comObject;
			var stopwatch = Stopwatch.StartNew ();
			do {
				comObject = GetComObject (monikerName);
				if (comObject != null)
					break;

				System.Threading.Thread.Sleep (100);
			}

			while (stopwatch.Elapsed < retryTimeout);

			return (T)comObject;
		}

		private static object GetComObject (string monikerName)
		{
			object comObject = null;
			try {
				IRunningObjectTable table;
				IEnumMoniker moniker;
				if (ErrorHandler.Failed (NativeMethods.GetRunningObjectTable (0, out table)))
					return null;

				table.EnumRunning (out moniker);
				moniker.Reset ();
				var pceltFetched = IntPtr.Zero;
				var rgelt = new IMoniker[1];

				while (moniker.Next (1, rgelt, pceltFetched) == 0) {
					IBindCtx ctx;
					if (!ErrorHandler.Failed (NativeMethods.CreateBindCtx (0, out ctx))) {
						string displayName;
						rgelt[0].GetDisplayName (ctx, null, out displayName);
						if (displayName == monikerName) {
							table.GetObject (rgelt[0], out comObject);
							return comObject;
						}
					}
				}
			} catch {
				return null;
			}

			return comObject;
		}
	}
}
