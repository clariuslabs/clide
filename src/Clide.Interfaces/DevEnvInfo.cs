using System;
using System.Collections.Generic;
using System.Linq;

namespace Clide
{
    /// <summary>
    /// Provides environmental information about the current instance of Visual Studio
    /// </summary>
    public class DevEnvInfo
    {
        /// <summary>
        /// Gets the Visual Studio channel identifier
        /// </summary>
        public string ChannelId { get; internal set; }

        /// <summary>
        /// Gets the Visual Studio channel title
        /// </summary>
        public string ChannelTitle { get; internal set; }

        /// <summary>
        /// Gets the Visual Studio edition
        /// </summary>
        public string Edition { get; internal set; }

        /// <summary>
        /// Gets the Visual Studio installation ID
        /// </summary>
        public string InstallationID { get; internal set; }

        /// <summary>
        /// Gets the Visual Studio installation name
        /// </summary>
        public string InstallationName { get; internal set; }

        /// <summary>
        /// Gets the Visual Studio installation version
        /// </summary>
        public Version Version { get; internal set; }

        /// <summary>
        /// Gets the Visual Studio display version
        /// </summary>
        public string DisplayVersion { get; internal set; }

        /// <summary>
        /// Gets the installed workloads
        /// </summary>
        public IEnumerable<string> Workloads { get; internal set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets the installed packages
        /// </summary>
        public IEnumerable<string> Packages { get; internal set; } = Enumerable.Empty<string>();
    }
}
