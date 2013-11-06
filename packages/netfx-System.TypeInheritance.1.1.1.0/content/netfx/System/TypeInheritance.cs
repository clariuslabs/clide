#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
#pragma warning disable 0436
using System;
using System.Collections.Generic;

/// <summary>
/// Provides type inheritance information for a type.
/// </summary>
internal class TypeInheritance : IEquatable<TypeInheritance>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TypeInheritance"/> class for the given 
	/// type and distance from the type root.
	/// </summary>
	public TypeInheritance(Type type, int distance)
	{
		Guard.NotNull(() => type, type);

		this.Distance = distance;
		this.Inheritance = new List<TypeInheritance>();
		this.Type = type;
	}

	/// <summary>
	/// Gets the distance from the current type inheritance to the root that was used 
	/// to build the hierarchy.
	/// </summary>
	public int Distance { get; private set; }

	/// <summary>
	/// Gets the type that owns the <see cref="Inheritance"/>.
	/// </summary>
	public Type Type { get; private set; }

	/// <summary>
	/// Gets the inherited types from <see cref="Type"/>.
	/// </summary>
	public List<TypeInheritance> Inheritance { get; private set; }

	#region Equality

	/// <summary>
	/// Compares the current instance with the value provided.
	/// </summary>
	public bool Equals(TypeInheritance other)
	{
		return TypeInheritance.Equals(this, other);
	}

	/// <summary>
	/// Compares the current instance with the value provided.
	/// </summary>
	public override bool Equals(object obj)
	{
		return TypeInheritance.Equals(this, obj as TypeInheritance);
	}

	/// <summary>
	/// Compares two instances for equality.
	/// </summary>
	public static bool Equals(TypeInheritance obj1, TypeInheritance obj2)
	{
		if (Object.Equals(null, obj1) ||
			Object.Equals(null, obj2) ||
			obj1.GetType() != obj2.GetType())
			return false;

		if (Object.ReferenceEquals(obj1, obj2)) return true;

		return obj1.Type == obj2.Type &&
			obj1.Distance == obj2.Distance;
	}

	/// <summary>
	/// Returns a hash code for this instance.
	/// </summary>
	public override int GetHashCode()
	{
		return this.Type.GetHashCode() ^ this.Distance.GetHashCode();
	}

	#endregion
}
#pragma warning restore 0436