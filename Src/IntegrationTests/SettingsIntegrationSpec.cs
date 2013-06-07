#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell.Settings;
    using System.ComponentModel;

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
    using System.ComponentModel;
    
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
