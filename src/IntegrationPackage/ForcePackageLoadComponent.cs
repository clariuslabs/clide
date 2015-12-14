using Clide.CommonComposition;
using System.ComponentModel.Composition;

namespace IntegrationPackage
{
	[Component]
	public class ForcePackageLoadComponent
	{
		public ForcePackageLoadComponent(IShellComponent package)
		{
			// No need to do anything special with the package here, 
			// although we might use state from it if needed.
		}
	}
}
