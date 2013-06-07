using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Clide;
using System.ComponentModel.DataAnnotations;

namespace IntegrationPackage
{
	[Settings]
	public class FooSettings : Settings
	{
		public FooSettings(ISettingsManager manager)
			: base(manager)
		{
		}

        [Required]
		public string DisplayName { get; set; }
        public Uri Address { get; set; }
        public int Port { get; set; }
	}
}
