using System;
using System.ComponentModel.Composition;
namespace Clide
{

    /// <summary>
    /// Attribute to apply to classes that implement the <see cref="ISettings"/> interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class SettingsAttribute : ExportAttribute
    {
        public SettingsAttribute()
            : base(typeof(ISettings))
        {

        }
    }
}
