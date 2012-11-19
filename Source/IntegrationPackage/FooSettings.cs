using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Clide;

namespace IntegrationPackage
{
	[Settings]
	public class FooSettings : Settings
	{
		[ImportingConstructor]
		public FooSettings(ISettingsManager manager)
			: base(manager)
		{
		}

		public string DisplayName { get; set; }

		protected override void OnSaveChanges()
		{
			base.OnSaveChanges();
		}
	}
}
