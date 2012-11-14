using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide
{
	public static class IDevEnvExtensions
	{
		public static T ToolWindow<T>(this IDevEnv environment) where T : IToolWindow
		{
			Guard.NotNull(() => environment, environment);

			return environment.ToolWindows.OfType<T>().FirstOrDefault();
		}
	}
}
