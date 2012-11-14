using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Clide
{
	/// <summary>
	/// Attribute to apply to classes that implement the <see cref="ISettings"/> interface.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class SettingsAttribute : InheritedExportAttribute
	{
		public SettingsAttribute()
		{

		}

		public SettingsAttribute(Type settingsType)
			: base(settingsType)
		{
		}

		///// <summary>
		///// Initializes a new instance of the <see cref="SettingsAttribute"/> class.
		///// </summary>
		//public SettingsAttribute()
		//    : base(typeof(ISettings))
		//{
		//}
	}
}
