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

namespace Clide.Events
{
    using System;

    /// <summary>
    /// Exposes solution-level events.
    /// </summary>
    public interface ISolutionEvents : IGlobalEvents
	{
        /// <summary>
        /// Occurs when a project is opened.
        /// </summary>
        event EventHandler<ProjectEventArgs> ProjectOpened;

        /// <summary>
        /// Occurs when a project is being closed.
        /// </summary>
        event EventHandler<ProjectEventArgs> ProjectClosing;

        /// <summary>
        /// Occurs when a solution is opened.
        /// </summary>
        event EventHandler SolutionOpened;

        /// <summary>
        /// Occurs when a solution is being closed.
        /// </summary>
        event EventHandler SolutionClosing;

        /// <summary>
        /// Occurs when a solution was closed.
        /// </summary>
        event EventHandler SolutionClosed;
	}
}
