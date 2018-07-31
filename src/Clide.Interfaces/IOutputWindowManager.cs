using System;
using System.IO;
namespace Clide
{

    /// <summary>
    /// Provides access to writing messages to the output window.
    /// </summary>
    public interface IOutputWindowManager : IFluentInterface
    {
        /// <summary>
        /// Gets or creates the output window pane with the given identifier and title.
        /// </summary>
        /// <param name="id">The identifier of the output window pane.</param>
        /// <param name="title">The title of the output window pane.</param>
        /// <returns>A <see cref="TextWriter"/> that can be used to write to the output window.</returns>
        TextWriter GetPane(Guid id, string title);
    }
}
