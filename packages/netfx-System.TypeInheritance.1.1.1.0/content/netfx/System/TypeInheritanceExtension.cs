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
using System.Linq;

/// <summary>
/// Provides the <see cref="GetInheritanceTree(Type)"/> extension method to retrieve 
/// inheritance tree information for a type.
/// </summary>
internal static class TypeInheritanceExtension
{
    /// <summary>
    /// Gets the exact inheritance tree information for the given type. The first element 
    /// in the inheritance is the received type itself.
    /// </summary>
    public static TypeInheritance GetInheritanceTree(this Type type)
    {
        return GetInheritanceTree(type, 0);
    }

    private static TypeInheritance GetInheritanceTree(this Type type, int distance)
    {
        var list = new TypeInheritance(type, distance);
        // Gives us a map of Interface + All ancestor interfaces in the entire hierarchy up.
        var interfaces = type
            .GetInterfaces()
            .Select(i => new { Interface = i, Ancestors = i.GetInterfaces().Traverse(TraverseKind.BreadthFirst, n => n.GetInterfaces()) });

        if (type.IsClass)
        {
            if (type.BaseType != null)
                list.Inheritance.Add(GetInheritanceTree(type.BaseType, distance + 1));

            list.Inheritance.AddRange(type
                // Add all interfaces of the type, but
                .GetInterfaces()
                // See if the map gives us where the interface members are implemented
                .Select(i => new { Interface = i, Map = type.GetInterfaceMap(i) })
                // Either it is a marker interface, or all members are declared by the type.
                // (explicit interface implementation or otherwise we are the first class in the hierarchy to introduce the interface).
                .Where(i =>
                    // Detect marker interfaces separately, and add them always as long as they don't show up in others upward.
                    (!i.Map.TargetMethods.Any() && !interfaces.SelectMany(n => n.Ancestors).Any(t => t == i.Interface)) ||
                        // For interfaces with members, we can get the map and check if they are declared in the current type.
                        // Note that this brings into this type the interfaces that have a completely overriden implementation (our intended design).
                    (i.Map.TargetMethods.Any() && i.Map.TargetMethods.All(m => m.DeclaringType == type)))
                .Select(i => GetInheritanceTree(i.Interface, distance + 1)));
        }
        else
        {
            // Then we only add those interfaces that do not show up as ancestors in any other 
            // interface in the list.
            list.Inheritance.AddRange(interfaces
                .Select(i => i.Interface)
                .Where(i => !interfaces.SelectMany(n => n.Ancestors).Any(t => t == i))
                .Select(i => GetInheritanceTree(i, distance + 1)));
        }

        return list;
    }
}
#pragma warning restore 0436