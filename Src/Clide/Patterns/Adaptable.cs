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

namespace Clide.Patterns.Adapter
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Default implementation of <see cref="IAdaptable{TSource}"/>.
	/// </summary>
	/// <typeparam name="TSource">The type of the object being adapted.</typeparam>
	public class Adaptable<TSource> : IAdaptable<TSource>
		where TSource : class
	{
		private IAdapterService service;
		private TSource source;

		/// <summary>
		/// Initializes a new instance of the <see cref="Adaptable{TSource}"/> class.
		/// </summary>
		/// <param name="service">The service that looks up adapter implementations.</param>
		/// <param name="source">The source object being adapted.</param>
		public Adaptable(IAdapterService service, TSource source)
		{
			this.service = service;
			this.source = source;
		}

		/// <summary>
		/// Adapts the instance to the given target type.
		/// </summary>
		/// <returns>The adapted instance or <see langword="null"/> if no compatible adapter was found.</returns>
		public T As<T>() where T : class
		{
			return this.service.Adapt<TSource>(source).As<T>();
		}
	}
}
