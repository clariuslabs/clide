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

namespace Clide.VisualStudio
{
    using Microsoft.VisualStudio.Shell.Interop;
	using System;

    /// <summary>
    /// Represents the combination of a Visual Studio hierarchy 
    /// and an item identifier.
    /// </summary>
    public class VsHierarchyItem : IEquatable<VsHierarchyItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VsHierarchyItem"/> class.
        /// </summary>
        /// <param name="hierarchy">The hierarchy.</param>
        /// <param name="itemId">The item id.</param>
        public VsHierarchyItem(IVsHierarchy hierarchy, uint itemId)
        {
            this.Hierarchy = hierarchy;
            this.ItemId = itemId;
        }

        /// <summary>
        /// Gets the hierarchy.
        /// </summary>
        public IVsHierarchy Hierarchy { get; private set; }

        /// <summary>
        /// Gets the item id.
        /// </summary>
        public uint ItemId { get; private set; }

		#region Equality

		/// <summary>
		/// Gets whether the given hierarchy items are equal.
		/// </summary>
		public static bool operator ==(VsHierarchyItem obj1, VsHierarchyItem obj2)
		{
			return Equals(obj1, obj2);
		}

		/// <summary>
		/// Gets whether the given hierarchy items are not equal.
		/// </summary>
		public static bool operator !=(VsHierarchyItem obj1, VsHierarchyItem obj2)
		{
			return !Equals(obj1, obj2);
		}

		/// <summary>
		/// Gets whether this hierarchy item is equal to the given one.
		/// </summary>
		public bool Equals(VsHierarchyItem other)
		{
			return VsHierarchyItem.Equals(this, other);
		}

		/// <summary>
		/// Gets whether this hierarchy item is equal to the given one.
		/// </summary>
		public override bool Equals(object obj)
		{
			return VsHierarchyItem.Equals(this, obj as VsHierarchyItem);
		}

		/// <summary>
		/// Gets whether the given hierarchy items are equal.
		/// </summary>
		public static bool Equals(VsHierarchyItem obj1, VsHierarchyItem obj2)
		{
			if (Object.Equals(null, obj1) ||
				Object.Equals(null, obj2) ||
				obj1.GetType() != obj2.GetType())
				return false;

			if (Object.ReferenceEquals(obj1, obj2)) return true;

			return obj1.Hierarchy == obj2.Hierarchy &&
				obj1.ItemId == obj2.ItemId;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Hierarchy.GetHashCode() ^ ItemId.GetHashCode();
		}

		#endregion
    }
}
