using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace IntegrationPackage
{
	/// <summary>
	/// Marker interface implemented by the package that 
	/// components can import to force the package to load
	/// before exports are used.
	/// </summary>
	public interface IShellPackage
	{
        ICompositionService Composition { get; }
	}
}
