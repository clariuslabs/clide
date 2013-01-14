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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;

/// <summary>
/// Provides convenience factory methods for <see cref="AmbientSingleton{T}"/> 
/// so that type inference can be leveraged for the given default value. There 
/// is no need to specify the T parameter for the Create method overloads.
/// </summary>
static partial class AmbientSingleton
{
    /// <summary>
    /// Creates an ambient singleton with no default value and a specific identifier.
    /// </summary>
    /// <typeparam name="T">Type of value held by the singleton.</typeparam>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public static AmbientSingleton<T> Create<T>(string identifier)
    {
        return new AmbientSingleton<T>(default(T), identifier);
    }

    /// <summary>
    /// Creates an ambient singleton with no default value and a specific identifier.
    /// </summary>
    /// <typeparam name="T">Type of value held by the singleton.</typeparam>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public static AmbientSingleton<T> Create<T>(Guid identifier)
    {
        return new AmbientSingleton<T>(identifier);
    }

    /// <summary>
	/// Creates an ambient singleton with the specified default value.
	/// </summary>
	/// <typeparam name="T">Type of value held by the singleton. No need to specify it explicitly.</typeparam>
	/// <param name="defaultValue">The default value for the singleton.</param>
	public static AmbientSingleton<T> Create<T>(T defaultValue)
	{
		return new AmbientSingleton<T>(defaultValue);
	}

	/// <summary>
	/// Creates an ambient singleton with the specified default value factory.
	/// </summary>
	/// <typeparam name="T">Type of value held by the singleton. No need to specify it explicitly.</typeparam>
	/// <param name="defaultValueFactory">The default value factory for the singleton.</param>
	public static AmbientSingleton<T> Create<T>(Func<T> defaultValueFactory)
	{
		return new AmbientSingleton<T>(defaultValueFactory);
	}

    /// <summary>
    /// Creates an ambient singleton with the specified default value and identifier.
    /// </summary>
    /// <typeparam name="T">Type of value held by the singleton. No need to specify it explicitly.</typeparam>
    /// <param name="defaultValue">The default value for the singleton.</param>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public static AmbientSingleton<T> Create<T>(T defaultValue, Guid identifier)
    {
        return new AmbientSingleton<T>(defaultValue, identifier);
    }

    /// <summary>
    /// Creates an ambient singleton with the specified default value and identifier.
    /// </summary>
    /// <typeparam name="T">Type of value held by the singleton. No need to specify it explicitly.</typeparam>
    /// <param name="defaultValue">The default value for the singleton.</param>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public static AmbientSingleton<T> Create<T>(T defaultValue, string identifier)
    {
        return new AmbientSingleton<T>(defaultValue, identifier);
    }

    /// <summary>
    /// Creates an ambient singleton with the specified default value factory.
    /// </summary>
    /// <typeparam name="T">Type of value held by the singleton. No need to specify it explicitly.</typeparam>
    /// <param name="defaultValueFactory">The default value factory for the singleton.</param>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public static AmbientSingleton<T> Create<T>(Func<T> defaultValueFactory, Guid identifier)
    {
        return new AmbientSingleton<T>(defaultValueFactory, identifier);
    }

    /// <summary>
    /// Creates an ambient singleton with the specified default value factory.
    /// </summary>
    /// <typeparam name="T">Type of value held by the singleton. No need to specify it explicitly.</typeparam>
    /// <param name="defaultValueFactory">The default value factory for the singleton.</param>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public static AmbientSingleton<T> Create<T>(Func<T> defaultValueFactory, string identifier)
    {
        return new AmbientSingleton<T>(defaultValueFactory, identifier);
    }
}

/// <summary>
/// Provides an easy way to implement the singleton (anti?) pattern so that it is ambient-safe,
/// propagates with a call context and can be overriden per ambient (i.e. in tests).
/// </summary>
/// <typeparam name="T">The type of value exposed as an ambient singleton.</typeparam>
/// <remarks>
/// This class is used to implement singletons that can be replaced in tests and are thread-safe 
/// for that scenario. A default value can be provided as a fallback if no ambient-specific value 
/// has been set prior to usage (i.e. a default singleton implementation).
/// <example>
/// The following example shows how to use the ambient singleton to define a singleton 
/// clock:
/// <code>
/// public class SystemClock : IClock
/// {
///		private static AmbientSingleton&lt;IClock&gt; singleton;
///			
///		static SystemClock()
///		{
///			singleton = new AmbientSingleton&lt;IClock&gt;(new SystemClock());
///		}
///	
///		private SystemClock()
///		{
///			// Can only be instantiated once and only by us.
///		}
/// 
///		public static IClock Instance 
///		{ 
///			get { return singleton.Value; } 
///			// Made internal so that only our tests can replace this value
///			internal set { singleton.Value = value; }
///		}
///		
///		public DateTimeOffset Now { get { return DateTimeOffset.Now; } }
///	}
/// </code>
/// A consumer domain class might use like as follows:
/// <code>
/// var now = SystemClock.Instance.Now;
/// </code>
/// A test could replace the value of Now by simply replacing the singleton:
/// <code>
/// SystemClock.Instance = mockClock;
/// 
/// // Would now use the mocked clock automatically from the replaced 
/// // ambient singleton
/// obj.PerformOperation();
/// </code>
/// </example>
/// </remarks>
/// <nuget id="netfx-System.AmbientSingleton"/>
partial class AmbientSingleton<T>
{
	private string slotName;
	private Lazy<T> defaultValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbientSingleton&lt;T&gt;"/> class 
    /// without a local default and a specific identifier.
    /// </summary>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public AmbientSingleton(Guid identifier)
        : this(() => default(T), identifier)
    {
    }

	/// <summary>
	/// Initializes a new instance of the <see cref="AmbientSingleton&lt;T&gt;"/> class 
	/// without a local default, meaning that if no value is assigned 
	/// to the <see cref="Value"/> property, it will return the default 
	/// value for the type.
	/// </summary>
	public AmbientSingleton()
        : this(() => default(T), Guid.NewGuid())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AmbientSingleton&lt;T&gt;"/> class 
	/// with a global default value. This value will be returned by the <see cref="Value"/> 
	/// property if no other value has been set in the current call context.
	/// </summary>
    /// <param name="defaultValue">The default value for the singleton.</param>
    public AmbientSingleton(T defaultValue)
        : this(() => defaultValue, Guid.NewGuid())
	{
	}

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbientSingleton&lt;T&gt;"/> class 
    /// with a global default value. This value will be returned by the <see cref="Value"/> 
    /// property if no other value has been set in the current call context.
    /// </summary>
    /// <param name="defaultValue">The default value for the singleton.</param>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public AmbientSingleton(T defaultValue, Guid identifier)
        : this(() => defaultValue, identifier)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbientSingleton&lt;T&gt;"/> class 
    /// with a global default value. This value will be returned by the <see cref="Value"/> 
    /// property if no other value has been set in the current call context.
    /// </summary>
    /// <param name="defaultValue">The default value for the singleton.</param>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public AmbientSingleton(T defaultValue, string identifier)
        : this(() => defaultValue, identifier)
    {
    }

    /// <summary>
	/// Initializes a new instance of the <see cref="AmbientSingleton&lt;T&gt;"/> class 
	/// with a global default value factory. This factory will be called once the first 
	/// time the global default value is accessed, such as if no other value has been 
	/// set in the current call context for the <see cref="Value"/> property.
	/// </summary>
    /// <param name="defaultValueFactory">The default value factory for the singleton.</param>
    public AmbientSingleton(Func<T> defaultValueFactory)
        : this(defaultValueFactory, Guid.NewGuid())
	{
	}

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbientSingleton&lt;T&gt;"/> class 
    /// with a global default value factory. This factory will be called once the first 
    /// time the global default value is accessed, such as if no other value has been 
    /// set in the current call context for the <see cref="Value"/> property.
    /// </summary>
    /// <param name="defaultValueFactory">The default value factory for the singleton.</param>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public AmbientSingleton(Func<T> defaultValueFactory, Guid identifier)
        : this(defaultValueFactory, identifier.ToString())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbientSingleton&lt;T&gt;"/> class 
    /// with a global default value factory. This factory will be called once the first 
    /// time the global default value is accessed, such as if no other value has been 
    /// set in the current call context for the <see cref="Value"/> property.
    /// </summary>
    /// <param name="defaultValueFactory">The default value factory for the singleton.</param>
    /// <param name="identifier">An identifier for the created singleton. Allows to reuse the ambient "variable" if needed.</param>
    public AmbientSingleton(Func<T> defaultValueFactory, string identifier)
    {
        Guard.NotNull(() => defaultValueFactory, defaultValueFactory);
        Guard.NotNullOrEmpty(() => identifier, identifier);

        this.defaultValue = new Lazy<T>(defaultValueFactory);
        this.slotName = identifier;
    }

    /// <summary>
	/// Gets or sets the value of the ambient singleton.
	/// </summary>
	/// <remarks>
	/// Setting the value will only change the specified 
	/// default value in the constructor for the current 
	/// call context.
	/// </remarks>
	public T Value
	{
		get 
		{
			var contextValue = CallContext.LogicalGetData(this.slotName);
            if (contextValue != null)
                return (T)contextValue;
            else
                // Set the value on first use. This allows the default 
                // to be seen by other ambient singletons reusing the 
                // same identifier.
                this.Value = this.defaultValue.Value;

			return this.defaultValue.Value;
		}
		set
		{
			CallContext.LogicalSetData(this.slotName, value);
		}
	}
}