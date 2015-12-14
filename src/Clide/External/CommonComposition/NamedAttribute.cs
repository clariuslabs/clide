namespace Clide.CommonComposition
{
    using System;

    /// <summary>
    /// Optional attribute that can be applied to components or dependencies 
    /// to specify the name of component or dependency.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NamedAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the component or dependency.</param>
        public NamedAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw new ArgumentOutOfRangeException("name");

            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the component or dependency.
        /// </summary>
        public string Name { get; private set; }
    }
}
