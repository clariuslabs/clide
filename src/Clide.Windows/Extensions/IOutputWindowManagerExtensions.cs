using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
namespace Clide
{

    /// <summary>
    /// Usability overloads for <see cref="IOutputWindowManager"/>.
    /// </summary>
    public static class IOutputWindowManagerExtensions
    {
        /// <summary>
        /// Gets the output window pane corresponding to the package, using the package
        /// <c>GuidAttribute</c> attribute as the pane identifier. The pane title is
        /// determined by the package class <c>DisplayNameAttribute</c> or its namespace
        /// if no display name is provided.
        /// </summary>
        /// <param name="manager">The output window manager.</param>
        /// <param name="package">The owning package.</param>
        /// <returns></returns>
        public static TextWriter GetPane(this IOutputWindowManager manager, IServiceProvider package)
        {
            var title = package.GetType().GetCustomAttributes<DisplayNameAttribute>()
                .Select(d => d.DisplayName)
                .FirstOrDefault() ?? package.GetType().Namespace;

            return GetPane(manager, package, title);
        }

        /// <summary>
        /// Gets the output window pane corresponding to the package, using the package 
        /// <c>GuidAttribute</c> attribute as the pane identifier.
        /// </summary>
        /// <param name="manager">The output window manager.</param>
        /// <param name="package">The owning package.</param>
        /// <param name="title">The title of the pane.</param>
        public static TextWriter GetPane(this IOutputWindowManager manager, IServiceProvider package, string title)
        {
            var id = package.GetPackageGuidOrThrow();

            return manager.GetPane(id, title);
        }

    }
}
