using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide.IntegrationPackage
{
	/// <summary>
	/// Marker interface implemented by the package that 
	/// components can import to force the package to load
	/// before exports are used.
	/// </summary>
	public interface ISamplePackage
	{
	}
}
