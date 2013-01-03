using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Clide;

namespace IntegrationPackage
{
	/// <summary>
	/// Component that forces a package load when imported.
	/// </summary>
	public interface IShellComponent
	{
        IDevEnv DevEnv { get; }
	}
}
