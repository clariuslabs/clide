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
