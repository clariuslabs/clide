using System;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    /// <summary>
    /// Provides extensions for Visual Studio low-level <see cref="IVsHierarchy"/>.
    /// </summary>
    internal static class IVsHierarchyExtensions
    {
        /// <summary>
        /// Gets the inner hierarchy of the flavored proejct
        /// when the actual hierarchy of the item is an instance of <see cref="FlavoredProjectBase"/>
        /// </summary>
        public static bool TryGetInnerHierarchy(this IVsHierarchy hierarchy, out IVsHierarchy innerHierarchy)
        {
            innerHierarchy = null;

            if (hierarchy is FlavoredProjectBase)
            {
                innerHierarchy = hierarchy
                     .GetType()
                     .GetField("_innerVsHierarchy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?
                     .GetValue(hierarchy) as IVsHierarchy;
            }

            return innerHierarchy != null;
        }
    }
}