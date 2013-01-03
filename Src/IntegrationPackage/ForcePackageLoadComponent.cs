using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace IntegrationPackage
{
	[Export]
	public class ForcePackageLoadComponent
	{
		[ImportingConstructor]
		public ForcePackageLoadComponent(IShellComponent package)
		{
			// No need to do anything special with the package here, 
			// although we might use state from it if needed.
		}
	}
}
