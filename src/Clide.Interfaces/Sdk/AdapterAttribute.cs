using System;
using System.ComponentModel.Composition;

namespace Clide.Sdk
{
    /// <summary>
    /// Decorates a component that implements <see cref="IAdapter{TFrom, TTo}"/> 
    /// conversion/s (multiple conversions are supported in a single component for convenience).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class AdapterAttribute : ExportAttribute
    {
        /// <summary>
        /// Initializes the attribute.
        /// </summary>
        public AdapterAttribute()
            : base(typeof(IAdapter))
        {
        }
    }
}
