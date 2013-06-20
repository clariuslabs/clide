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
    using System.Collections.Generic;

    /// <summary>
    /// General-purpose tree node interface used by all supported tree structures in VS.
    /// </summary>
    public interface ITreeNode : IFluentInterface
	{
        /// <summary>
        /// Gets the node display name.
        /// </summary>
		string DisplayName { get; }

        /// <summary>
        /// Gets a value indicating whether this node is hidden.
        /// </summary>
        bool IsHidden { get; }

        /// <summary>
        /// Gets a value indicating whether this node is visible.
        /// </summary>
		bool IsVisible { get; }

        /// <summary>
        /// Gets a value indicating whether this node is selected.
        /// </summary>
		bool IsSelected { get; }

        /// <summary>
        /// Gets a value indicating whether this node is expanded.
        /// </summary>
        bool IsExpanded { get; }

        /// <summary>
        /// Gets the parent of this node.
        /// </summary>
		ITreeNode Parent { get; }

        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        IEnumerable<ITreeNode> Nodes { get; }

        /// <summary>
        /// Tries to smart-cast this node to the give type.
        /// </summary>
        /// <typeparam name="T">Type to smart-cast to.</typeparam>
        /// <returns>The casted value or null if it cannot be converted to that type.</returns>
		T As<T>() where T : class;

        /// <summary>
        /// Collapses this node.
        /// </summary>
		void Collapse();

        /// <summary>
        /// Expands the node, optionally in a recursive fashion.
        /// </summary>
        /// <param name="recursively">if set to <c>true</c>, expands recursively</param>
		void Expand(bool recursively = false);

        /// <summary>
        /// Selects the node, optionally allowing multiple selection.
        /// </summary>
        /// <param name="allowMultiple">if set to <c>true</c>, adds this node to the current selection.</param>
        void Select(bool allowMultiple = false);
	}
}
