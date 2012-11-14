namespace Clide.Commands
{
    using System;

    /// <summary>
    /// Manages commands in the environment.
    /// </summary>
    public interface ICommandManager
    {
        /// <summary>
        /// Adds the command of the given type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the command, which
        /// must implement the <see cref="ICommandExtension"/> interface and be annotated with 
        /// the <see cref="CommandAttribute"/> attribute.</typeparam>
        /// <remarks>
        /// The command can import additional services, which are satisfied right after creation.
        /// </remarks>
        void AddCommand<T>() where T : ICommandExtension, new();

        /// <summary>
        /// Adds the specified command implementation to the manager.
        /// </summary>
        /// <param name="command">The command instance, which must be annotated with 
        /// the <see cref="CommandAttribute"/> attribute.</param>
        /// <remarks>
        /// The manager performs a satisfy imports call for pre-instantiated commands 
        /// so that property dependencies are set.
        /// </remarks>
        void AddCommand(ICommandExtension command);

        /// <summary>
        /// Adds the specified command implementation to the manager, 
        /// with the specified explicit metadata.
        /// </summary>
        /// <param name="command">The command instance, which does not need to 
        /// be annotated with the <see cref="CommandAttribute"/> attribute since 
        /// it's provided explicitly.</param>
        /// <param name="metadata">Explicit metadata to use for the command, 
        /// instead of reflecting the <see cref="CommandAttribute"/>.</param>
        /// <remarks>
        /// The manager performs a satisfy imports call for pre-instantiated commands 
        /// so that property dependencies are set.
        /// </remarks>
        void AddCommand(ICommandExtension command, ICommandMetadata metadata);

        /// <summary>
        /// Adds all the commands that have been annotated with the <see cref="CommandAttribute"/> with 
        /// a package identifier that matches the <see cref="GuidAttribute"/> 
        /// on the given <paramref name="owningPackage"/>.
        /// </summary>
        void AddCommands(IServiceProvider owningPackage);

        /// <summary>
        /// Adds the command filter of the given type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the command filter, which
        /// must implement the <see cref="ICommandFilter"/> interface and be annotated with 
        /// the <see cref="CommandFilterAttribute"/> attribute.</typeparam>
        /// <remarks>
        /// The command can import additional services, which are satisfied right after creation.
        /// </remarks>
        void AddFilter<T>() where T : ICommandFilter, new();

        /// <summary>
        /// Adds the specified command filter implementation to the manager.
        /// </summary>
        /// <param name="filter">The command filter instance, which must be annotated with 
        /// the <see cref="CommandFilterAttribute"/> attribute.</param>
        /// <remarks>
        /// The manager performs a satisfy imports call for pre-instantiated commands 
        /// so that property dependencies are set.
        /// </remarks>
        void AddFilter(ICommandFilter filter);

        /// <summary>
        /// Adds the specified command filter implementation to the manager, 
        /// with the specified explicit metadata.
        /// </summary>
        /// <param name="command">The command filter instance, which does not need to 
        /// be annotated with the <see cref="CommandFilterAttribute"/> attribute since 
        /// it's provided explicitly.</param>
        /// <param name="metadata">Explicit metadata to use for the command, 
        /// instead of reflecting the <see cref="CommandFilterAttribute"/>.</param>
        /// <remarks>
        /// The manager performs a satisfy imports call for pre-instantiated commands 
        /// so that property dependencies are set.
        /// </remarks>
        void AddFilter(ICommandFilter filter, ICommandFilterMetadata metadata);

        /// <summary>
        /// Adds all the commands filters that have been annotated with the <see cref="CommandFilterAttribute"/> with 
        /// a package identifier that matches the <see cref="GuidAttribute"/> 
        /// on the given <paramref name="owningPackage"/>.
        /// </summary>
        void AddFilters(IServiceProvider owningPackage);
    }
}
