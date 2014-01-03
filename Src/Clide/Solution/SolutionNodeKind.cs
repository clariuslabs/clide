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
    /// The kind of solution node.
    /// </summary>
	public enum SolutionNodeKind
	{
        /// <summary>
        /// The node is the solution node.
        /// </summary>
		Solution,

        /// <summary>
        /// The node is a solution folder.
        /// </summary>
		SolutionFolder,

        /// <summary>
        /// The node is a solution item, meaning it 
        /// exists in a solution folder, not  a project.
        /// </summary>
        SolutionItem,

        /// <summary>
        /// The node is a project.
        /// </summary>
        Project,

        /// <summary>
        /// The node is a project folder.
        /// </summary>
		Folder,

        /// <summary>
        /// The node is a project item.
        /// </summary>
        Item,

        /// <summary>
        /// The node is a reference in a project.
        /// </summary>
		Reference,

        /// <summary>
        /// The node is the references folder.
        /// </summary>
		ReferencesFolder,

        /// <summary>
        /// The node is of a custom kind.
        /// </summary>
		Custom,
	}
}
