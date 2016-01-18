using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Clide
{
	public static class GlobalServiceLocator
	{
		public static IServiceLocator Instance { get; }

		static GlobalServiceLocator()
		{
			Instance = new ServiceLocator (GlobalServices.Instance);
		}
	}
}
