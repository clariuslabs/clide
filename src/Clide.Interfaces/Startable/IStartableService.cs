using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Clide
{
    /// <summary>
    /// Service that allows to start components that matches a given context
    /// </summary>
    public interface IStartableService
    {
        /// <summary>
        /// Starts all the components that matches the given context
        /// </summary>
        /// <param name="context">The context string to filter the components that need to be started</param>
        /// <returns></returns>
        Task StartComponentsAsync(string context);

        /// <summary>
        /// Starts all the components that matches the given context
        /// </summary>
        /// <param name="context">The context string to filter the components that need to be started</param>
        /// <param name="cancellationToken">The token to cancel the ongoing work</param>
        /// <returns></returns>
        Task StartComponentsAsync(string context, CancellationToken cancellationToken);
    }
}
