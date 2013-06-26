using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Clide;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.ComponentModel;

namespace IntegrationPackage
{
	[Settings]
	public class FooSettings : Settings
	{
		public FooSettings(ISettingsManager manager)
			: base(manager)
		{
            this.BarSettings = new BarSettings();
		}

        [Required]
		public string DisplayName { get; set; }
        public Uri Address { get; set; }
        public int Port { get; set; }

        // TODO: add support for child properties
        public BarSettings BarSettings { get; private set; }
	}

    public class BarSettings
    {
        public BarSettings()
        {
            this.TraceSources = new BindingList<SourceSettings>();
        }

        [DefaultValue(SourceLevels.Warning)]
        public SourceLevels RootSourceLevel { get; set; }
        // TODO: add support for nested collection values
        public ICollection<SourceSettings> TraceSources { get; private set; }
    }

    public class SourceSettings
    {
        public string SourceName { get; set; }
        public SourceLevels LoggingLevel { get; set; }
    }
}
