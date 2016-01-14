using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	/// <summary>
	/// Provides extensions for Visual Studio low-level <see cref="IVsSolution"/>.
	/// </summary>
	internal static class IVsSolutionExtensions
	{
		public static object GetProperty (this IVsSolution solution, int propId, object defaultValue = null)
		{
			return GetProperty<object> (solution, propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsSolution solution, __VSPROPID propId, T defaultValue = default (T))
		{
			return solution.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsSolution solution, __VSPROPID2 propId, T defaultValue = default (T))
		{
			return solution.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsSolution solution, __VSPROPID3 propId, T defaultValue = default (T))
		{
			return solution.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsSolution solution, __VSPROPID4 propId, T defaultValue = default (T))
		{
			return solution.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsSolution solution, __VSPROPID5 propId, T defaultValue = default (T))
		{
			return solution.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsSolution solution, VsHierarchyPropID propId, T defaultValue = default (T))
		{
			return solution.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsSolution solution, int propId, T defaultValue = default (T))
		{
			object value = null;

			if (ErrorHandler.Succeeded (solution.GetProperty(propId, out value)) &&
				value != null) {
				return (T)value;
			}

			return defaultValue;
		}
	}
}
