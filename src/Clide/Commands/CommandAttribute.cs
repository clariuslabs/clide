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

namespace Clide.Commands
{
    using Clide.CommonComposition;
    using Clide.Composition;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Composition;

    /// <summary>
    /// Attribute that must be placed on command implementations in order to 
    /// use the <see cref="ICommandManager.AddCommands"/> method.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : ComponentAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class 
        /// from a dictionary of values
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CommandAttribute(IDictionary<string, object> attributes)
            : this((string)attributes.GetOrAdd("PackageId", _ => Guid.Empty.ToString()), (string)attributes["GroupId"], (int)attributes["CommandId"])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
        /// </summary>
		/// <devdoc>
		/// Made EditorBrowsableState.Never so that we don't make the mistake of not using it.
		/// All commands should have the package ID.
		/// </devdoc>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CommandAttribute(string groupGuid, int commandId)
			: this(Guid.Empty.ToString(), groupGuid, commandId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
        /// </summary>
        public CommandAttribute(string packageGuid, string groupGuid, int commandId)
        {
			this.PackageId = packageGuid;
            this.GroupId = groupGuid;
            this.CommandId = commandId;
        }

        /// <summary>
        /// Optional identifier for the package that owns/exposes the given command. 
		/// If ommited, all commands will be registered.
        /// </summary>
        public string PackageId { get; private set; }

        /// <summary>
        /// Gets the group GUID.
        /// </summary>
        public string GroupId { get; private set; }

        /// <summary>
        /// Gets the command id.
        /// </summary>
        public int CommandId { get; private set; }
    }
}
