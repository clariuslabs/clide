using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Clide
{
    static class DteExtensions
    {
        /// <summary>
        /// Returns the <see cref="Project.UniqueName"/> or its <see cref="Project.FullName"/> if 
        /// the first fails.
        /// </summary>
        public static string GetUniqueNameOrFullName(this Project project)
        {
            try
            {
                // This might throw if the project isn't loaded yet.
                return project.UniqueName;
            }
            catch (Exception)
            {
                // As a fallback, in C#/VB, the UniqueName == FullName.
                // It may still fail in the ext call though, but we do our best
                return project.FullName;
            }
        }
    }
}
