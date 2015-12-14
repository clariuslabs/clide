namespace Clide.CommonComposition
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Marks the decorated class as a component that will be registered
    /// for composition
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ComponentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentAttribute"/> class, 
        /// marking the decorated class as a component that will be registered 
        /// for composition.
        /// </summary>
        public ComponentAttribute()
        {
            IsSingleton = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this component should be treated as a singleton 
        /// or single instance within a given composition scope (i.e. a container). 
        /// </summary>
        /// <remarks>
        /// Defaults to <see langword="false"/>, meaning every component depending on the 
        /// annotated class will be given a new instance of it within a given scope, 
        /// rather than being reused.
        /// </remarks>
        [DefaultValue(false)]
        public bool IsSingleton { get; set; }
    }
}