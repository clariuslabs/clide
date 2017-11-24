using System;

namespace Clide
{
	public interface ICommandManager
	{
		void RegisterCommands(IServiceProvider package);
	}
}