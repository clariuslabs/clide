using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Linq;

namespace Clide.Commands
{
	[Export(typeof(ICommandRegistrar))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	class CommandRegistrar : ICommandRegistrar
	{
		readonly Lazy<IMenuCommandService> menuCommandService;
		readonly ConcurrentDictionary<Guid, List<Lazy<ICommandExtension, ICommandMetadata>>> commandsByPackage =
			new ConcurrentDictionary<Guid, List<Lazy<ICommandExtension, ICommandMetadata>>>();

		[ImportingConstructor]
		public CommandRegistrar(
			[Import(ContractNames.Interop.IMenuCommandService)] Lazy<IMenuCommandService> menuCommandService,
			[ImportMany(CommandAttribute.CommandContractName)] IEnumerable<Lazy<ICommandExtension, ICommandMetadata>> commands)
		{
			this.menuCommandService = menuCommandService;

			foreach (var groupedCommands in commands.GroupBy(x => x.Metadata.PackageId))
				commandsByPackage.GetOrAdd(
					new Guid(groupedCommands.Key), x => groupedCommands.ToList());
		}

		public void RegisterCommands(IServiceProvider package) =>
			RegisterCommands(package.GetType().GUID);

		public void RegisterCommands(Guid packageGuid)
		{
			List<Lazy<ICommandExtension, ICommandMetadata>> commands;
			if (commandsByPackage.TryGetValue(packageGuid, out commands))
				foreach (var command in commands)
					menuCommandService.Value.AddCommand(
						new VsCommandExtensionAdapter(
							new CommandID(new Guid(command.Metadata.GroupId), command.Metadata.CommandId),
							command.Value));
		}
	}
}