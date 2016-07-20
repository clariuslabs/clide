using System;
using System.ComponentModel.Composition;

namespace Clide
{
	/// <summary>
	/// Marks the decorated class as a component that will be registered
	/// for composition in the Visual Studio container. Classes annotated 
	/// with this attribute must be partial.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class ComponentAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ComponentAttribute"/> class, 
		/// marking the decorated class as a component that will be registered 
		/// for composition.
		/// </summary>
		public ComponentAttribute(CreationPolicy creationPolicy)
		{
			CreationPolicy = creationPolicy;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this component should be treated as a singleton 
		/// or single instance (<see cref="CreationPolicy.Shared"/>) within a given composition scope (i.e. a container).
		/// </summary>
		public CreationPolicy CreationPolicy { get; private set; }
	}
}
