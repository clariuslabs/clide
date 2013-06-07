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

namespace Clide.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.Composition;
    using Clide.Commands;
    using System.ComponentModel;
    using Clide.Composition;

    /// <summary>
    /// Attribute that must be placed on command interceptors in order to 
    /// use the <see cref="ICommandManager.AddInterceptors"/> method.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandInterceptorAttribute : ComponentAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInterceptorAttribute"/> class 
        /// from a dictionary of values
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CommandInterceptorAttribute(IDictionary<string, object> attributes)
            : this((string)attributes["PackageId"], (string)attributes["GroupId"], (int)attributes["CommandId"])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInterceptorAttribute" /> class.
        /// </summary>
        /// <param name="packageGuid">Gets the GUID of the package that provides the command to intercept.</param>
        /// <param name="groupGuid">The group GUID of the intercepted command.</param>
        /// <param name="commandId">The command id of the intercepted command.</param>
        public CommandInterceptorAttribute(string packageGuid, string groupGuid, int commandId)
            : base(typeof(ICommandInterceptor))
        {
            this.PackageId = packageGuid;
            this.GroupId = groupGuid;
            this.CommandId = commandId;
        }

        /// <summary>
        /// Gets the GUID of the package that provides the command to intercept.
        /// </summary>
        public string PackageId { get; private set; }

        /// <summary>
        /// Gets the command id of the command to intercept.
        /// </summary>
        public int CommandId { get; private set; }
        
        /// <summary>
        /// Gets the group id of the command to intercept.
        /// </summary>
        public string GroupId { get; private set; }
    }
}
