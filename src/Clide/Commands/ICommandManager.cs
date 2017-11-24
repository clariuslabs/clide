using System;

namespace Clide.Commands
{
	public interface ICommandManager
	{
		void RegisterCommands(IServiceProvider package);
	}
}