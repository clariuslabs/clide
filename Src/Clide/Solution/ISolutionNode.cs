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
    using Clide.Events;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the solution root node in the solution explorer tree.
    /// </summary>
    public interface ISolutionNode : ISolutionExplorerNode, ISolutionEvents
	{
        /// <summary>
        /// Gets a value indicating whether a solution is open.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Gets the currently selected nodes in the solution.
        /// </summary>
        IEnumerable<ISolutionExplorerNode> SelectedNodes { get; }

        /// <summary>
        /// Closes the solution.
        /// </summary>
        /// <param name="saveFirst">If set to <c>true</c> saves the solution before closing.</param>
        void Close(bool saveFirst = true);

        /// <summary>
        /// Creates a new blank solution with the specified solution file location.
        /// </summary>
        void Create(string solutionFile);

        /// <summary>
        /// Opens the specified solution file.
        /// </summary>
        void Open(string solutionFile);

        /// <summary>
        /// Saves the current solution.
        /// </summary>
        void Save();

        /// <summary>
        /// Saves the current solution to the specified target file.
        /// </summary>
        void SaveAs(string solutionFile);

        /// <summary>
        /// Creates a solution folder under the solution root.
        /// </summary>
		ISolutionFolderNode CreateSolutionFolder(string name);
    }
}
