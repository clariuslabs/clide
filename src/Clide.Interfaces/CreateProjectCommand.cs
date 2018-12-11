using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Merq;

namespace Clide
{
    /// <summary>
    /// Creates a new project using the .Net Core CLI (dotnet new) and return the list of created/unfolded project files
    /// </summary>
    public class CreateProjectCommand : ICommand<IEnumerable<string>>
    {
        /// <summary>
        /// The template to instantiate
        /// </summary>
        [Required]
        public string Template { get; set; }

        /// <summary>
        /// Installs a source or template pack from the PATH or NUGET_ID provided
        /// </summary>
        public string Install { get; set; }

        /// <summary>
        /// Specifies a NuGet source to use during install.
        /// </summary>
        public string NuGetSource { get; set; }

        /// <summary>
        /// Forces content to be generated even if it would change existing files. This is required when the output directory already contains a project.
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        /// The name for the created output. If no name is specified, the name of the current directory is used.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Location to place the generated output. The default is the current directory.
        /// </summary>
        [Required]
        public string Output { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// The language of the template to create.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Additional options that the template might support
        /// </summary>
        public Dictionary<string, string> AdditionalOptions { get; set; } = new Dictionary<string, string>();
    }
}