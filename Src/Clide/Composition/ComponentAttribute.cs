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

namespace Clide.Composition
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Marks the decorated class as a component that will be available from 
    /// the service locator / component container, by default as a singleton 
    /// component (see <see cref="IsSingleton"/>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ComponentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentAttribute"/> class, 
        /// marking the decorated class as a component that will be available from 
        /// the service locator / component container, registered with all 
        /// implemented interfaces as well as the concrete type.
        /// </summary>
        public ComponentAttribute()
        {
            this.RegisterAs = new Type[0];
            this.IsSingleton = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentAttribute"/> class, 
        /// marking the decorated class as a component that will be available from 
        /// the service locator / component container using the specified 
        /// <paramref name="registerAs"/> type.
        /// </summary>
        /// <param name="registerAs">The type to use to register the decorated component.</param>
        public ComponentAttribute(Type registerAs)
            : this(new [] { registerAs })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentAttribute"/> class, 
        /// marking the decorated class as a component that will be available from 
        /// the service locator / component container using the specified 
        /// <paramref name="registerAs"/> type or types.
        /// </summary>
        /// <param name="registerAs">The type or types to use to register the decorated component.</param>
        public ComponentAttribute(params Type[] registerAs)
        {
            this.RegisterAs = registerAs;
            this.IsSingleton = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentAttribute"/> class, 
        /// marking the decorated class as a component that will be available from 
        /// the service locator / component container using the specified 
        /// <paramref name="registerKey"/>.
        /// </summary>
        /// <param name="registerKey">The key to use to identify the decorated component.</param>
        public ComponentAttribute(object registerKey)
        {
            this.RegisterKey = registerKey;
            this.RegisterAs = new Type[0];
            this.IsSingleton = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentAttribute"/> class, 
        /// marking the decorated class as a component that will be available from 
        /// the service locator / component container using the specified 
        /// <paramref name="registerAs"/> type and <paramref name="registerKey"/>.
        /// </summary>
        /// <param name="registerKey">The key to use to identify the decorated component.</param>
        /// <param name="registerAs">The type to use to register the decorated component.</param>
        public ComponentAttribute(object registerKey, Type registerAs)
        {
            this.RegisterKey = registerKey;
            this.RegisterAs = new [] { registerAs };
            this.IsSingleton = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentAttribute"/> class 
        /// from a metadata dictionary.
        /// </summary>
        /// <param name="metadata">The metadata to initialize the attribute from.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ComponentAttribute(IDictionary<string, object> metadata)
        {
            this.RegisterAs = new Type[0];
            this.IsSingleton = true;

            object value;
            if (metadata.TryGetValue("RegisterKey", out value))
                this.RegisterKey = value;
            if (metadata.TryGetValue("RegisterAs", out value))
                this.RegisterAs = (Type[])value;
            if (metadata.TryGetValue("IsSingleton", out value))
                this.IsSingleton = (bool)value;
        }

        /// <summary>
        /// Gets the key to use to register the component, if any.
        /// </summary>
        public object RegisterKey { get; private set; }

        /// <summary>
        /// Gets the type to register the component as, if any.
        /// </summary>
        public Type[] RegisterAs { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this component should be treated as a singleton 
        /// or single instance within the container. Defaults to <see langword="true"/>.
        /// </summary>
        [DefaultValue(true)]
        public bool IsSingleton { get; set; }
    }
}