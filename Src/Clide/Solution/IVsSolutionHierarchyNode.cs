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
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Low-level primitive that exposes the underlying 
    /// Visual Studio hierarchy nodes.
    /// </summary>
    public interface IVsSolutionHierarchyNode
	{
        /// <summary>
        /// Gets the service provider.
        /// </summary>
		IServiceProvider ServiceProvider { get; }
        
        /// <summary>
        /// Gets the node hierarchy.
        /// </summary>
		IVsHierarchy VsHierarchy { get; }

        /// <summary>
        /// Gets the extensibility object.
        /// </summary>
        object ExtensibilityObject { get; }

        /// <summary>
        /// Gets the item id.
        /// </summary>
        uint ItemId { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the parent node.
        /// </summary>
        IVsSolutionHierarchyNode Parent { get; }

        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        IEnumerable<IVsSolutionHierarchyNode> Children { get; }
	}
}
