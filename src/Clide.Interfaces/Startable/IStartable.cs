using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clide
{
    /// <summary>
    /// Represents a component that needs to be started by the <see cref="IStartableService"/>
    /// </summary>
    public interface IStartable
    {
        /// <summary>
        /// Runs the startup logic for the component
        /// </summary>
        Task StartAsync();
    }
}