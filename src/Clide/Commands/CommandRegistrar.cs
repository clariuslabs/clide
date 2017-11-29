using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using Clide.Properties;

namespace Clide.Commands
{
	[Export(typeof(ICommandRegistrar))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	class CommandRegistrar : ICommandRegistrar
	{
		static readonly ITracer tracer = Tracer.Get<CommandRegistrar>();

		readonly Lazy<ConcurrentDictionary<Guid, List<Lazy<ICommandExtension, ICommandMetadata>>>> commandsByPackage;

		[ImportingConstructor]
		public CommandRegistrar([ImportMany] IEnumerable<Lazy<ICommandExtension, ICommandMetadata>> commands)
		{
			commandsByPackage = new Lazy<ConcurrentDictionary<Guid, List<Lazy<ICommandExtension, ICommandMetadata>>>>(
				() =>
				{
					var result = new ConcurrentDictionary<Guid, List<Lazy<ICommandExtension, ICommandMetadata>>>();

					foreach (var groupedCommands in commands.GroupBy(x => x.Metadata.PackageId))
					{
						try
						{
							result
								.AddOrUpdate(
									new Guid(groupedCommands.Key),
									key => new List<Lazy<ICommandExtension, ICommandMetadata>>(groupedCommands),
									(key, value) =>
									{
										value.AddRange(groupedCommands);
										return value;
									});
						}
						catch (Exception ex)
						{
							tracer.Error(ex, Strings.CommandRegistrar.ErrorImportingCommandForPackage(groupedCommands.Key));
						}
					}

					return result;
				});
		}

		public void RegisterCommands(IServiceProvider package)
		{
			var packageGuid = package.GetType().GUID;

			// Using dynamic to avoid referencing Shell.11
			var menuCommandService = package.GetService<IMenuCommandService>() as dynamic;

			List<Lazy<ICommandExtension, ICommandMetadata>> commands;
			if (commandsByPackage.Value.TryGetValue(packageGuid, out commands))
				foreach (var command in commands)
					menuCommandService.AddCommand(
						new VsCommandExtensionAdapter(
							new CommandID(new Guid(command.Metadata.GroupId), command.Metadata.CommandId),
							command.Value));
		}
	}
}