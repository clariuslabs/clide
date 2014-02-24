using Clide;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IntegrationPackage
{
	[Guid("2935B3DB-0274-4033-8FF6-14634C7FBAC2")]
	public class FooSettingsManager : Component, Microsoft.VisualStudio.Shell.IProfileManager
	{
		public FooSettingsManager()
		{

		}

		public void LoadSettingsFromStorage()
		{
		}

		public void LoadSettingsFromXml(Microsoft.VisualStudio.Shell.Interop.IVsSettingsReader reader)
		{
		}

		public void ResetSettings()
		{
		}

		public void SaveSettingsToStorage()
		{
		}

		public void SaveSettingsToXml(Microsoft.VisualStudio.Shell.Interop.IVsSettingsWriter writer)
		{
			//writer.
		}
	}

	[Settings]
	public class FooSettings : Settings
	{
		public FooSettings(ISettingsManager manager)
			: base(manager)
		{
			this.BarSettings = new BarSettings();
		}

		public Uri Address { get; set; }

		[DefaultValue(8080)]
		[Required]
		[Range(8000, 50000)]
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
