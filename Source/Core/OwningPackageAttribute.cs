using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Clide
{
	/// <summary>
	/// Used to specify the package that owns a given component, such as a <see cref="ToolsOptionsPage{TControl, TSettings}"/> 
	/// derived class.
	/// </summary>
	[MetadataAttribute]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public class OwningPackageAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OwningPackageAttribute"/> class.
		/// </summary>
		public OwningPackageAttribute(string packageGuid)
		{
			this.PackageId = new Guid(packageGuid);
		}

		/// <summary>
		/// Gets the package GUID.
		/// </summary>
		public Guid PackageId { get; private set; }
	}
}
