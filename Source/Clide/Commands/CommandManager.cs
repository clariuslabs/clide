#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
namespace Clide.Commands
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Design;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Clide.Composition;
    using Clide.Properties;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Implements the command registration mechanism.
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICommandManager))]
    internal class CommandManager : ICommandManager
    {
        private IVsShell vsShell;
        private Lazy<ICompositionService> composition;
        private IEnumerable<Lazy<ICommandExtension, ICommandMetadata>> allCommands;
        private IEnumerable<Lazy<ICommandFilter, ICommandFilterMetadata>> allFilters;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandManager"/> class.
        /// </summary>
        [ImportingConstructor]
        public CommandManager(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [Import(VsContractNames.IVsShell)] IVsShell vsShell,
            [Import(ContractNames.ICompositionService)] Lazy<ICompositionService> composition,
            [ImportMany] IEnumerable<Lazy<ICommandExtension, ICommandMetadata>> allCommands,
            [ImportMany] IEnumerable<Lazy<ICommandFilter, ICommandFilterMetadata>> allFilters)
        {
            this.ServiceProvider = serviceProvider;
            this.vsShell = vsShell;
            this.allCommands = allCommands;
            this.allFilters = allFilters;
            this.composition = composition;
        }

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        /// <devdoc>Made internal to allow replacing for testing.</devdoc>
        internal IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Adds the command of the given type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the command, which
        /// must implement the <see cref="ICommandExtension"/> interface and be annotated with
        /// the <see cref="CommandAttribute"/> attribute.</typeparam>
        public void AddCommand<T>() where T : ICommandExtension, new()
        {
            var command = new T();
            this.composition.Value.SatisfyImportsOnce(command);

            AddCommand(command);
        }

        /// <summary>
        /// Adds the specified command implementation to the manager.
        /// </summary>
        /// <param name="command">The command instance, which must be annotated with
        /// the <see cref="CommandAttribute"/> attribute.</param>
        public void AddCommand(ICommandExtension command)
        {
            Guard.NotNull(() => command, command);

            var metadata = GetCommandMetadataOrThrow(command.GetType());
            AddCommand(command, metadata);
        }

        /// <summary>
        /// Adds the specified command implementation to the manager,
        /// with the specified explicit metadata.
        /// </summary>
        /// <param name="command">The command instance, which does not need to
        /// be annotated with the <see cref="CommandAttribute"/> attribute since
        /// it's provided explicitly.</param>
        /// <param name="metadata">Explicit metadata to use for the command,
        /// instead of reflecting the <see cref="CommandAttribute"/>.</param>
        public void AddCommand(ICommandExtension command, ICommandMetadata metadata)
        {
            Guard.NotNull(() => command, command);
            Guard.NotNull(() => metadata, metadata);

            var services = GetPackageOrThrow(command.GetType(), new Guid(metadata.PackageId));
            var menuService = services.GetService<IMenuCommandService>();

            menuService.AddCommand(new VsCommandExtensionAdapter(new CommandID(new Guid(metadata.GroupId), metadata.CommandId), command));
        }

        /// <summary>
        /// Adds all the commands that have been annotated with the <see cref="CommandAttribute"/> with
        /// a package identifier that matches the <see cref="GuidAttribute"/>
        /// on the given <paramref name="owningPackage"/>.
        /// </summary>
        public void AddCommands(IServiceProvider owningPackage)
        {
            Guard.NotNull(() => owningPackage, owningPackage);

            var packageGuid = GetPackageGuidOrThrow(owningPackage);
            var menuService = owningPackage.GetService<IMenuCommandService>();
            var packageCommands = this.allCommands
                .Where(command => new Guid(command.Metadata.PackageId) == packageGuid);

            foreach (var command in packageCommands)
            {
                AddCommand(command.Value, command.Metadata);
            }
        }

        /// <summary>
        /// Adds the command filter of the given type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the command filter, which
        /// must implement the <see cref="ICommandFilter"/> interface and be annotated with
        /// the <see cref="CommandFilterAttribute"/> attribute.</typeparam>
        public void AddFilter<T>() where T : ICommandFilter, new()
        {
            var filter = new T();
            this.composition.Value.SatisfyImportsOnce(filter);

            AddFilter(filter);
        }

        /// <summary>
        /// Adds the specified command filter implementation to the manager.
        /// </summary>
        /// <param name="filter">The command filter instance, which must be annotated with
        /// the <see cref="CommandFilterAttribute"/> attribute.</param>
        public void AddFilter(ICommandFilter filter)
        {
            Guard.NotNull(() => filter, filter);

            var metadata = GetFilterMetadataOrThrow(filter.GetType());
            AddFilter(filter, metadata);
        }

        /// <summary>
        /// Adds the specified command filter implementation to the manager,
        /// with the specified explicit metadata.
        /// </summary>
        /// <param name="filter">The command filter instance, which does not need to
        /// be annotated with the <see cref="CommandFilterAttribute"/> attribute since
        /// it's provided explicitly.</param>
        /// <param name="metadata">Explicit metadata to use for the command filter,
        /// instead of reflecting the <see cref="CommandFilterAttribute"/>.</param>
        public void AddFilter(ICommandFilter filter, ICommandFilterMetadata metadata)
        {
            Guard.NotNull(() => filter, filter);
            Guard.NotNull(() => metadata, metadata);

            var commandPackageGuid = new Guid(metadata.PackageId);
            var commandPackage = default(IVsPackage);
            vsShell.IsPackageLoaded(ref commandPackageGuid, out commandPackage);

            if (commandPackage == null)
                ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref commandPackageGuid, out commandPackage));

            // TODO: trace all these failure conditions.
            var serviceProvider = commandPackage as IServiceProvider;
            if (serviceProvider == null)
                return;

            var mcs = serviceProvider.GetService<IMenuCommandService>();
            if (mcs != null)
            {
                var command = mcs.FindCommand(new CommandID(new Guid(metadata.GroupId), metadata.CommandId)) as OleMenuCommand;
                if (command != null)
                {
                    command.BeforeQueryStatus += (sender, args) => filter.QueryStatus(new OleMenuCommandAdapter((OleMenuCommand)sender));
                }
            }
        }

        /// <summary>
        /// Adds all the command filters that have been annotated with the <see cref="CommandFilterAttribute"/> with
        /// an owning package identifier that matches the <see cref="GuidAttribute"/>
        /// on the given <paramref name="owningPackage"/>.
        /// </summary>
        public void AddFilters(IServiceProvider owningPackage)
        {
            Guard.NotNull(() => owningPackage, owningPackage);

            var owningPackageGuid = GetPackageGuidOrThrow(owningPackage);                        
            var packageFilters = this.allFilters
                .Where(filter => new Guid(filter.Metadata.OwningPackageId) == owningPackageGuid);

            var vsShell = owningPackage.GetService<SVsShell, IVsShell>();

            foreach (var filter in packageFilters)
            {
                AddFilter(filter.Value, filter.Metadata);
            }
        }

        private ICommandMetadata GetCommandMetadataOrThrow(Type type)
        {
            var cmd = type.GetCustomAttribute<CommandAttribute>();
            if (cmd == null)
                throw new ArgumentException(Strings.CommandManager.CommandAttributeMissing(type));

            return new CommandMetadata
            {
                PackageId = cmd.PackageId,
                GroupId = cmd.GroupId,
                CommandId = cmd.CommandId
            };
        }

        private ICommandFilterMetadata GetFilterMetadataOrThrow(Type type)
        {
            var cmd = type.GetCustomAttribute<CommandFilterAttribute>();
            if (cmd == null)
                throw new ArgumentException(Strings.CommandManager.CommandFilterAttributeMissing(type));

            return new CommandFilterMetadata
            {
                OwningPackageId = cmd.OwningPackageId,
                PackageId = cmd.PackageId,
                GroupId = cmd.GroupId,
                CommandId = cmd.CommandId
            };
        }

        private static Guid GetPackageGuidOrThrow(IServiceProvider owningPackage)
        {
            var guid = owningPackage.GetType().GetCustomAttribute<GuidAttribute>(true);
            if (guid == null)
                throw new ArgumentException(Strings.CommandManager.PackageGuidMissing(owningPackage.GetType()));

            return new Guid(guid.Value);
        }

        private IServiceProvider GetPackageOrThrow(Type command, Guid packageGuid)
        {
            var guid = packageGuid;
            var package = default(IVsPackage);

            this.vsShell.IsPackageLoaded(ref guid, out package);

            if (package == null)
                ErrorHandler.ThrowOnFailure(this.vsShell.LoadPackage(ref guid, out package));

            if (package == null)
                throw new InvalidOperationException(Strings.CommandManager.OwningPackageNotFound(command, packageGuid));

            var services = package as IServiceProvider;

            if (services == null)
                throw new InvalidOperationException(Strings.CommandManager.OwningPackageNotServiceProvider(command, packageGuid));

            return services;
        }

        private class CommandMetadata : ICommandMetadata
        {
            public string PackageId { get; set; }
            public string GroupId { get; set; }
            public int CommandId { get; set; }
        }

        private class CommandFilterMetadata : CommandMetadata, ICommandFilterMetadata
        {
            public string OwningPackageId { get; set; }
        }
    }
}
