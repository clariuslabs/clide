using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace Clide
{
	/// <summary>
	/// Provides access to the Visual Studio global composition service.
	/// </summary>
	public static class ComponentModel
	{
		private static IComponentModel componentModelOverride;

		/// <summary>
		/// Gets the global component model. The global 
		/// service provider <see cref="ServiceProvider.GlobalProvider"/> 
		/// must be available before invoking this property.
		/// </summary>
		public static IComponentModel GlobalComponentModel
		{
			get
			{
				if (componentModelOverride != null)
					return componentModelOverride;

				if (ServiceProvider.GlobalProvider == null)
					throw new InvalidOperationException("No global service provider found.");

				return ServiceProvider.GlobalProvider.GetService<SComponentModel, IComponentModel>();
			}
			internal set { componentModelOverride = value; }
		}
	}
}
