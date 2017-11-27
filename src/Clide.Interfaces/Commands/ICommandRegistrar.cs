using System;

namespace Clide
{
	public interface ICommandRegistrar
	{
		void RegisterCommands(IServiceProvider package);
		void RegisterCommands(Guid packageGuid);
	}
}