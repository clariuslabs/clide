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

namespace Clide.Solution
{

    /// <summary>
    /// Represents a project in the solution explorer tree.
    /// </summary>
    public interface IProjectNode : ISolutionExplorerNode
	{
        /// <summary>
        /// Gets the project configuration information.
        /// </summary>
        IProjectConfiguration Configuration { get; }

        /// <summary>
        /// Creates a folder inside the project.
        /// </summary>
		IFolderNode CreateFolder(string name);

        /// <summary>
        /// Gets the physical path of the project.
        /// </summary>
		string PhysicalPath { get; }

        /// <summary>
        /// Saves pending changes to the project file.
        /// </summary>
        void Save();

        /// <summary>
        /// Gets the global properties of the project.
        /// </summary>
		dynamic Properties { get; }

        /// <summary>
        /// Gets the configuration-specific properties for the project.
        /// </summary>
        /// <param name="configurationName">Configuration names are the combination 
        /// of a project configuration and the platform, like "Debug|AnyCPU".</param>
        dynamic PropertiesFor(string configurationName);
	}
}
