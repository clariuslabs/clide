using System;
using System.ComponentModel.Composition;

namespace Clide
{
	/// <summary>
	/// Attribute applied to event producers that implement 
	/// <see cref="IObservable{T}"/> to export them to the 
	/// composition container.
	/// </summary>
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
	public class ObservableAttribute : ExportAttribute
	{
		/// <summary>
		/// Initializes the attribute.
		/// </summary>
		public ObservableAttribute () 
			: base(typeof(IObservable<>))
		{
		}
	}
}
