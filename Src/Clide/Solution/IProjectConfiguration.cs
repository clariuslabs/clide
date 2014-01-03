#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Solution
{
    using System.Collections.Generic;

    /// <summary>
    /// Project configuration information
    /// </summary>
    public interface IProjectConfiguration
    {
        /// <summary>
        /// Gets the combined configuration and platform that makes 
        /// the configuration name for use in <see cref="IProjectNode.PropertiesFor"/>, 
        /// such as "Debug|AnyCPU".
        /// </summary>
        string ActiveConfigurationName { get; }

        /// <summary>
        /// Gets the active project configuration.
        /// </summary>
        string ActiveConfiguration { get; }

        /// <summary>
        /// Gets the active target platform.
        /// </summary>
        string ActivePlatform { get; }

        /// <summary>
        /// Gets all the configuration names.
        /// </summary>
        IEnumerable<string> Configurations { get; }

        /// <summary>
        /// Gets all the target platform names.
        /// </summary>
        IEnumerable<string> Platforms { get; }
    }
}
