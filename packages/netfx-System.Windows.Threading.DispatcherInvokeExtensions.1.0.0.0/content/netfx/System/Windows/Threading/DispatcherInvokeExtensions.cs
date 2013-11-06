#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

/// <summary>
/// Helpers for easily invoking via the <see cref="Dispatcher"/> using 
/// an <see cref="Action"/> or a <see cref="Func{T}"/> delegate or lambda.
/// </summary>
///	<netfx id="netfx-System.Windows.Threading.DispatcherInvokeExtensions" />
internal static partial class DispatcherInvokeExtensions
{
	/// <summary>
	/// Executes the specified delegate asynchronously with the specified arguments
	///	on the thread that the System.Windows.Threading.Dispatcher was created on.
	/// </summary>
	/// <param name="dispatcher">The dispatcher to invoke the action on.</param>
	/// <param name="action">The action to execute via the dispatcher.</param>
	public static void BeginInvoke(this Dispatcher dispatcher, Action action)
	{
		Guard.NotNull(() => dispatcher, dispatcher);
		Guard.NotNull(() => action, action);

		dispatcher.BeginInvoke((Delegate)action);
	}

	/// <summary>
	/// Executes the specified delegate asynchronously with the specified arguments
	/// on the thread that the System.Windows.Threading.Dispatcher was created on.
	/// </summary>
	/// <param name="dispatcher">The dispatcher to invoke the action on.</param>
	/// <param name="action">The action to execute via the dispatcher.</param>
	/// <param name="priority">The priority to execute the action with.</param>
	public static void BeginInvoke(this Dispatcher dispatcher, Action action, DispatcherPriority priority)
	{
		Guard.NotNull(() => dispatcher, dispatcher);
		Guard.NotNull(() => action, action);

		dispatcher.BeginInvoke((Delegate)action, priority);
	}

	/// <summary>
	/// Executes the specified delegate with the specified arguments synchronously
	///	on the thread the System.Windows.Threading.Dispatcher is associated with.
	/// </summary>
	/// <param name="dispatcher">The dispatcher to invoke the action on.</param>
	/// <param name="action">The action to execute via the dispatcher.</param>
	public static void Invoke(this Dispatcher dispatcher, Action action)
	{
		Guard.NotNull(() => dispatcher, dispatcher);
		Guard.NotNull(() => action, action);
		
		dispatcher.Invoke((Delegate)action);
	}

	/// <summary>
	/// Executes the specified delegate with the specified arguments synchronously
	///	on the thread the System.Windows.Threading.Dispatcher is associated with.
	/// </summary>
	/// <param name="dispatcher">The dispatcher to invoke the action on.</param>
	/// <param name="action">The action to execute via the dispatcher.</param>
	/// <param name="priority">The priority to execute the action with.</param>
	public static void Invoke(this Dispatcher dispatcher, Action action, DispatcherPriority priority)
	{
		Guard.NotNull(() => dispatcher, dispatcher);
		Guard.NotNull(() => action, action);

		dispatcher.Invoke((Delegate)action, priority);
	}

	/// <summary>
	/// Executes the specified delegate with the specified arguments synchronously
	///	on the thread the System.Windows.Threading.Dispatcher is associated with.
	/// </summary>
	/// <param name="dispatcher">The dispatcher to invoke the action on.</param>
	/// <param name="action">The action to execute via the dispatcher.</param>
	/// <returns>The return value from the delegate being invoked</returns>
	public static T Invoke<T>(this Dispatcher dispatcher, Func<T> action)
	{
		Guard.NotNull(() => dispatcher, dispatcher);
		Guard.NotNull(() => action, action);

		return (T)dispatcher.Invoke((Delegate)action);
	}

	/// <summary>
	/// Executes the specified delegate with the specified arguments synchronously
	///	on the thread the System.Windows.Threading.Dispatcher is associated with.
	/// </summary>
	/// <param name="dispatcher">The dispatcher to invoke the action on.</param>
	/// <param name="action">The action to execute via the dispatcher.</param>
	/// <param name="priority">The priority to execute the action with.</param>
	/// <returns>The return value from the delegate being invoked</returns>
	public static T Invoke<T>(this Dispatcher dispatcher, Func<T> action, DispatcherPriority priority)
	{
		Guard.NotNull(() => dispatcher, dispatcher);
		Guard.NotNull(() => action, action);

		return (T)dispatcher.Invoke((Delegate)action, priority);
	}
}
