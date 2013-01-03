using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System.ComponentModel;

namespace Clide
{
	public class SettingsIntegrationSpecs : VsHostedSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[TestClass]
		public class GivenASimpleClass
		{
			private WritableSettingsStore settingsStore;

			[TestInitialize]
			public void Initialize()
			{
                var shellManager = new ShellSettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);
				this.settingsStore = shellManager.GetWritableSettingsStore(SettingsScope.UserSettings);

				var collection = SettingsManager.GetSettingsCollectionName(typeof(FooSettings));
				if (this.settingsStore.CollectionExists(collection))
					this.settingsStore.DeleteCollection(collection);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenCancellingEdit_ValuesShouldNotPersist()
			{
                var manager = new SettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);
				var foo = new FooSettings(manager);

				foo.BeginEdit();
				// change the current value
				foo.DefaultValueStringProperty = "WhenCancellingEdit_ValuesShouldNotPersist";
				foo.DefaultValueIntProperty = 65000;
				// cancel edit, should revert edit changes back to original values
				foo.CancelEdit();
				// check just edited values are not there anymore
				Assert.NotEqual("WhenCancellingEdit_ValuesShouldNotPersist", foo.DefaultValueStringProperty);
				Assert.NotEqual(65000, foo.DefaultValueIntProperty);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenEndingEdit_ValuesShouldPersist()
			{
                var manager = new SettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);
				var foo = new FooSettings(manager);

				foo.BeginEdit();
				foo.DefaultValueStringProperty = "WhenEndingEdit_ValuesShouldPersist";
				foo.DefaultValueIntProperty = 65001;
				foo.EndEdit();
				// reload from settings store
				manager.Read(foo);
				// check the just edited values are there
				Assert.Equal("WhenEndingEdit_ValuesShouldPersist", foo.DefaultValueStringProperty);
				Assert.Equal(65001, foo.DefaultValueIntProperty);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenPersisting_ShouldUseFullTypename()
			{
                var manager = new SettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);
				var foo = new FooSettings(manager);

				foo.BeginEdit();
				foo.DefaultValueStringProperty = "WhenEndingEdit_ValuesShouldPersist";
				foo.DefaultValueIntProperty = 65001;
				foo.EndEdit();

				var sameTypeName = new Clide.IntegrationTests.OtherNamespace.FooSettings(manager);
				sameTypeName.BeginEdit();
				sameTypeName.DefaultValueStringProperty = "abc";
				sameTypeName.DefaultValueIntProperty = 123;
				sameTypeName.EndEdit();

				// reload from settings store
				manager.Read(foo);
				manager.Read(sameTypeName);

				// check the just edited values are there
				Assert.Equal("WhenEndingEdit_ValuesShouldPersist", foo.DefaultValueStringProperty);
				Assert.Equal(65001, foo.DefaultValueIntProperty);
				Assert.Equal("abc", sameTypeName.DefaultValueStringProperty);
				Assert.Equal(123, sameTypeName.DefaultValueIntProperty);
			}


			public class FooSettings : Settings
			{
				public string StringProperty { get; set; }
				[DefaultValue("Hello")]
				public string DefaultValueStringProperty { get; set; }
				public int IntProperty { get; set; }
				[DefaultValue(5)]
				public int DefaultValueIntProperty { get; set; }
				[DefaultValue("25")]
				public int DefaultValueAsStringIntProperty { get; set; }

				public UriFormat EnumProperty { get; set; }
				[DefaultValue("Unescaped")]
				public UriFormat DefaultValueEnumProperty { get; set; }

				[DefaultValue("00:00:05")]
				public TimeSpan PingInterval { get; set; }

				public Bar ComplexTypeWithConverter { get; set; }

				public FooSettings(ISettingsManager manager)
					: base(manager)
				{
				}
			}

			[TypeConverter(typeof(BarConverter))]
			public class Bar
			{
				public Bar(string value)
				{
					this.Value = value;
				}

				public string Value { get; set; }

				private class BarConverter : TypeConverter
				{
					public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
					{
						return destinationType == typeof(string);
					}

					public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
					{
						return ((Bar)value).Value;
					}

					public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
					{
						return sourceType == typeof(string);
					}

					public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
					{
						return new Bar((string)value);
					}
				}
			}
		}
	}
}

namespace Clide.IntegrationTests.OtherNamespace
{
	public class FooSettings : Settings
	{
		public string StringProperty { get; set; }
		[DefaultValue("Hello")]
		public string DefaultValueStringProperty { get; set; }
		public int IntProperty { get; set; }
		[DefaultValue(5)]
		public int DefaultValueIntProperty { get; set; }
		[DefaultValue("25")]
		public int DefaultValueAsStringIntProperty { get; set; }

		public FooSettings(ISettingsManager manager)
			: base(manager)
		{
		}
	}
}
