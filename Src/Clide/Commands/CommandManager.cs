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
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Clide.Properties;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.CSharp.RuntimeBinder;
    using EnvDTE;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using Clide.Diagnostics;
    using Clide.Composition;

    /// <summary>
    /// Implements the command registration mechanism.
    /// </summary>
    [Component(typeof(ICommandManager))]
    internal class CommandManager : ICommandManager
    {
        private static readonly ITracer tracer = Tracer.Get<CommandManager>();

        private IServiceProvider serviceProvider;
        private IVsShell vsShell;
        private CommandEvents commandEvents;

        private IEnumerable<Lazy<ICommandExtension, CommandAttribute>> commands;
        private IEnumerable<Lazy<ICommandFilter, CommandFilterAttribute>> filters;
        private IEnumerable<Lazy<ICommandInterceptor, CommandInterceptorAttribute>> interceptors;

        private ConcurrentDictionary<Tuple<Guid, int>, List<ICommandInterceptor>> registeredInterceptors = new ConcurrentDictionary<Tuple<Guid, int>, List<ICommandInterceptor>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandManager"/> class.
        /// </summary>
        public CommandManager(
            IServiceProvider serviceProvider,
            IEnumerable<Lazy<ICommandExtension, CommandAttribute>> allCommands,
            IEnumerable<Lazy<ICommandFilter, CommandFilterAttribute>> allFilters,
            IEnumerable<Lazy<ICommandInterceptor, CommandInterceptorAttribute>> allInterceptors)
        {
            this.serviceProvider = serviceProvider;
            this.vsShell = serviceProvider.GetService<SVsShell, IVsShell>();
            this.commandEvents = serviceProvider.GetService<DTE>().Events.CommandEvents;
            this.commands = allCommands;
            this.filters = allFilters;
            this.interceptors = allInterceptors.ToList();

            this.commandEvents.BeforeExecute += OnBeforeExecute;
            this.commandEvents.AfterExecute += OnAfterExecute;
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
        public void AddCommand(ICommandExtension command, CommandAttribute metadata)
        {
            Guard.NotNull(() => command, command);
            Guard.NotNull(() => metadata, metadata);

            var menuService = serviceProvider.GetService<IMenuCommandService>();

            menuService.AddCommand(new VsCommandExtensionAdapter(new CommandID(new Guid(metadata.GroupId), metadata.CommandId), command));
            tracer.Info(Strings.CommandManager.CommandRegistered(command.Text, command.GetType()));
        }

        /// <summary>
        /// Adds all the commands that have been annotated with the <see cref="CommandAttribute"/>.
        /// </summary>
        public void AddCommands()
        {
            var menuService = serviceProvider.GetService<IMenuCommandService>();
            foreach (var command in commands)
            {
                AddCommand(command.Value, command.Metadata);
            }
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
        public void AddFilter(ICommandFilter filter, CommandFilterAttribute metadata)
        {
            Guard.NotNull(() => filter, filter);
            Guard.NotNull(() => metadata, metadata);

            var commandPackageGuid = new Guid(metadata.PackageId);
            var commandPackage = default(IVsPackage);
            vsShell.IsPackageLoaded(ref commandPackageGuid, out commandPackage);

            if (commandPackage == null)
                ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref commandPackageGuid, out commandPackage));

            var serviceProvider = commandPackage as IServiceProvider;
            if (serviceProvider == null)
            {
                tracer.Error(Strings.CommandManager.CommandPackageNotServiceProvider(commandPackageGuid));
                return;
            }

            var mcs = serviceProvider.GetService<IMenuCommandService>();
            if (mcs == null)
            {
                tracer.Error(Strings.CommandManager.NoMenuCommandService(commandPackageGuid));
                return;
            }

            var groupId = new Guid(metadata.GroupId);
            var command = mcs.FindCommand(new CommandID(groupId, metadata.CommandId));
            if (command == null)
            {
                tracer.Error(Strings.CommandManager.CommandNotFound(commandPackageGuid, groupId, metadata.CommandId));
                return;
            }

            // \o/: for some reason this cast never works on VS2012, even with the proper assembly references :(.
            // So we resort to dynamic.
            // var command =  as OleMenuCommand;
            dynamic dynCommand = command.AsDynamicReflection();
            try
            {
                dynCommand.add_BeforeQueryStatus(new EventHandler((sender, args) =>
                {
                    try
                    {
                        filter.QueryStatus(new OleMenuCommandAdapter((OleMenuCommand)sender));
                    }
                    catch (Exception e)
                    {
                        tracer.Error(Strings.CommandManager.FilterFailed(filter, e));
                    }
                }));
            }
            catch (RuntimeBinderException)
            {
                // The command may not be an OleMenuCommand and therefore it wouldn't have the BeforeQueryStatus.
                tracer.Error(Strings.CommandManager.CommandNotOle(commandPackageGuid, metadata.GroupId, metadata.CommandId));
            }
        }

        /// <summary>
        /// Adds all the command filters that have been annotated with the <see cref="CommandFilterAttribute"/>.
        /// </summary>
        public void AddFilters()
        {
            foreach (var filter in filters)
            {
                AddFilter(filter.Value, filter.Metadata);
            }
        }

        /// <summary>
        /// Adds the specified command interceptor implementation to the manager,
        /// with the specified explicit metadata.
        /// </summary>
        /// <param name="interceptor">The command interceptor instance, which does not need to
        /// be annotated with the <see cref="CommandInterceptorAttribute"/> attribute since
        /// it's provided explicitly.</param>
        /// <param name="metadata">Explicit metadata to use for the command interceptor,
        /// instead of reflecting the <see cref="CommandInterceptorAttribute"/>.</param>
        public void AddInterceptor(ICommandInterceptor interceptor, CommandInterceptorAttribute metadata)
        {
            Guard.NotNull(() => interceptor, interceptor);
            Guard.NotNull(() => metadata, metadata);

            var commandInterceptors = this.registeredInterceptors.GetOrAdd(
                Tuple.Create(new Guid(metadata.GroupId), metadata.CommandId),
                key => new List<ICommandInterceptor>());

            commandInterceptors.Add(interceptor);
        }

        /// <summary>
        /// Adds all the command interceptors that have been annotated with the <see cref="CommandInterceptorAttribute"/>.
        /// </summary>
        public void AddInterceptors()
        {
            foreach (var interceptor in interceptors)
            {
                AddInterceptor(interceptor.Value, interceptor.Metadata);
            }
        }

        private void OnBeforeExecute(string groupGuid, int commandId, object customIn, object customOut, ref bool CancelDefault)
        {
            ExecuteInterceptors(groupGuid, commandId, interceptor => interceptor.BeforeExecute());
        }

        private void OnAfterExecute(string groupGuid, int commandId, object customIn, object customOut)
        {
            ExecuteInterceptors(groupGuid, commandId, interceptor => interceptor.AfterExecute());
        }

        private void ExecuteInterceptors(string groupGuid, int commandId, Action<ICommandInterceptor> execute)
        {
            var interceptors = default(List<ICommandInterceptor>);
            if (this.registeredInterceptors.TryGetValue(Tuple.Create(new Guid(groupGuid), commandId), out interceptors))
            {
                foreach (var interceptor in interceptors)
                {
                    try
                    {
                        execute(interceptor);
                    }
                    catch (Exception e)
                    {
                        tracer.Error(Strings.CommandManager.InterceptorFailed(interceptor, e));
                    }
                }
            }
        }
    }
}
