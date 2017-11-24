using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Linq;

namespace Clide.Commands
{
	[Export(typeof(ICommandManager))]
	class CommandManager : ICommandManager
	{
		readonly Lazy<IMenuCommandService> menuCommandService;
		readonly Lazy<IServiceLocator> serviceLocator;

		[ImportingConstructor]
		public CommandManager(
			[Import(ContractNames.Interop.IMenuCommandService)] Lazy<IMenuCommandService> menuCommandService,
			Lazy<IServiceLocator> serviceLocator)
		{
			this.menuCommandService = menuCommandService;
			this.serviceLocator = serviceLocator;
		}

		public void RegisterCommands(IServiceProvider package)
		{
			var packageGuid = package.GetType().GUID;

			var commands = serviceLocator
				.Value
				.GetExports<ICommandExtension, ICommandMetadata>()
				.Where(x => new Guid(x.Metadata.PackageId) == packageGuid)
				.Select(x =>
					new VsCommandExtensionAdapter(
						new CommandID(new Guid(x.Metadata.GroupId), x.Metadata.CommandId),
						x.Value));

			foreach (var command in commands)
				menuCommandService.Value.AddCommand(command);
		}
	}
}