using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clide
{
    /// <summary>
    /// Solution configuration information
    /// </summary>
    public interface ISolutionConfiguration
    {
        /// <summary>
        /// Changes the platform in the current active solution configuration
        /// </summary>
        /// <param name="platform">The new platform name to be selected</param>
        /// <returns></returns>
        Task ChangePlatformAsync(string platform);
    }
}
