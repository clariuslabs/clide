#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide
{
    using System.Linq;
using Clide.Solution;

    /// <summary>
    /// Usability extensions for <see cref="IDevEnv"/>.
    /// </summary>
    public static class IDevEnvExtensions
	{
        /// <summary>
        /// Gets the tool window of the given type from the environment, if any.
        /// </summary>
        /// <returns>The tool window or <see langword="null"/> if it does not exist.</returns>
		public static T ToolWindow<T>(this IDevEnv environment) where T : IToolWindow
		{
			Guard.NotNull(() => environment, environment);

			return environment.ToolWindows.OfType<T>().FirstOrDefault();
		}

        /// <summary>
        /// Gets the solution explorer tool window.
        /// </summary>
        public static ISolutionExplorer SolutionExplorer(this IDevEnv environment)
        {
            return ToolWindow<ISolutionExplorer>(environment);
        }
	}
}
