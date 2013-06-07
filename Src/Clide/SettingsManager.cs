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
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Clide.Diagnostics;
    using Clide.Properties;
    using Clide.Composition;
    using Microsoft.VisualStudio.Settings;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Settings;

    /// <summary>
	/// Default implementation of <see cref="ISettingsManager"/> which uses <see cref="ShellSettingsManager"/>.
	/// </summary>
	[Component(typeof(ISettingsManager))]
	internal class SettingsManager : ISettingsManager
	{
		private static readonly ITracer tracer = Tracer.Get<SettingsManager>();

		private Lazy<ISettingsStore> settingsStore;

		/// <summary>
		/// Initializes a new instance of the <see cref="SettingsManager"/> class.
		/// </summary>
		/// <param name="serviceProvider">The service provider.</param>
		public SettingsManager(IServiceProvider serviceProvider)
			: this(serviceProvider, new Lazy<ISettingsStore>(() => new ShellSettingsStore(serviceProvider)))
		{
		}

		internal SettingsManager(IServiceProvider serviceProvider, Lazy<ISettingsStore> settingsStore)
		{
			Guard.NotNull(() => serviceProvider, serviceProvider);
			Guard.NotNull(() => settingsStore, settingsStore);

			this.ServiceProvider = serviceProvider;
			this.settingsStore = settingsStore;
		}

		internal IServiceProvider ServiceProvider { get; set; }

		public void Save(object settings, bool saveDefaults = false)
		{
			Guard.NotNull(() => settings, settings);

			// Always validates 
			Validator.ValidateObject(settings, new ValidationContext(settings, this.ServiceProvider, null));

			var store = this.settingsStore.Value;
			var collectionName = GetSettingsCollectionName(settings.GetType());

			using (tracer.StartActivity(string.Format("Saving settings to ", collectionName)))
			{
				// Recreate the settings.
				if (store.CollectionExists(collectionName))
					store.DeleteCollection(collectionName);

				store.CreateCollection(collectionName);

				foreach (var property in TypeDescriptor.GetProperties(settings).Cast<PropertyDescriptor>())
				{
					var value = property.GetValue(settings);
					if (value == null)
					{
						if (store.PropertyExists(collectionName, property.Name))
							store.DeleteProperty(collectionName, property.Name);
					}
					else
					{
						if (!ShouldSaveValue(property, value))
							continue;

						if (property.Converter.CanConvertTo(typeof(string)))
						{
							try
							{
								var stringValue = property.Converter.ConvertToString(value);
								store.SetString(collectionName, property.Name, stringValue);
							}
							catch (Exception)
							{
								var convertible = value as IConvertible;
								if (convertible != null)
								{
									try
									{
										var convertedString = (string)convertible.ToType(typeof(string), CultureInfo.CurrentCulture);
										store.SetString(collectionName, property.Name, convertedString);
									}
									catch (Exception)
									{
										if (property.Converter.CanConvertFrom(typeof(string)) && value.GetType().IsValueType)
										{
											// Best-guess calling ToString.
											store.SetString(collectionName, property.Name, value.ToString());
										}
									}
								}
								else if (property.Converter.CanConvertFrom(typeof(string)) && value.GetType().IsValueType)
								{
									// Best-guess calling ToString.
									store.SetString(collectionName, property.Name, value.ToString());
								}
								else
								{
									tracer.Warn(Strings.SettingsManager.CannotSaveAsString(value.GetType().Name, settings.GetType().Name, property.Name));
								}
							}
						}
						else
						{
							tracer.Warn(Strings.SettingsManager.CannotSaveAsString(value.GetType().Name, settings.GetType().Name, property.Name));
						}
					}
				}
			}
		}

		public void Read(object settings)
		{
			Guard.NotNull(() => settings, settings);

			var settingsType = settings.GetType();
			var store = this.settingsStore.Value;
			var collectionName = GetSettingsCollectionName(settingsType);

			using (tracer.StartActivity("Reading settings from {0}", collectionName))
			{
				var canceled = false;
				var editable = settings as IEditableObject;
				var initializable = settings as ISupportInitialize;

				if (initializable != null)
					initializable.BeginInit();

				if (editable != null)
					editable.BeginEdit();

				try
				{
					if (!store.CollectionExists(collectionName))
					{
						InitializeDefaultValues(settings);
					}

					// TODO: this could be optimized by caching.
					var mappedMethods = settingsType
						// Get all the interfaces implemented by the type
						.GetInterfaces()
						// The interfaces map contains the methods on the target class where the interface is implemented
						.Select(iface => settingsType.GetInterfaceMap(iface))
						// We need to shred this information into Property > getter maps, so we can retrieve the 
						// attributes associated with the property on the interface itself.
						.SelectMany(interfaceMap => interfaceMap.InterfaceMethods
							// We're only interested in property getters/setters.
							.Where(interfaceMethod => interfaceMethod.Name.StartsWith("get_"))
							// Get a map of interface method => target method that is easier to lookup.
							.Select(interfaceMethod => new
							{
								// Get the declaring property for the interface getter method.
								InterfaceProperty = interfaceMethod.DeclaringType.GetProperty(interfaceMethod.Name.Substring(4)),
								// Match the target getter on the class, which in C# spec is a match of Name + parameters.
								TargetMethod = interfaceMap.TargetMethods.FirstOrDefault(targetMethod =>
									targetMethod.Name == interfaceMethod.Name &&
									targetMethod.GetParameters().SequenceEqual(interfaceMethod.GetParameters()))
							})
							// Finally, filter out those where we couldn't find the property or the target getter.
							.Where(map => map.InterfaceProperty != null && map.TargetMethod != null));

					foreach (var property in TypeDescriptor.GetProperties(settings).Cast<PropertyDescriptor>().Where(p => !p.IsReadOnly))
					{
						if (store.PropertyExists(collectionName, property.Name))
						{
							// If the property exists in the store, we try to set it on the settings object.
							var value = store.GetString(collectionName, property.Name);
							if (value != null)
								TrySetValue(settings, property, value);
						}
						else
						{
							var defaultValue = property.Attributes.OfType<DefaultValueAttribute>().Select(x => x.Value).FirstOrDefault();
							// If attribute is not found, try going via reflection, to the mapped interfaces that have this member.
							if (defaultValue == null)
							{
								// Try to locate the property corresponding to the TypeDescriptor property. 
								// Note that we used TypeDescriptor to provide an extensibility hook for settings classes 
								// that want to be fancier and possibly expose fake properties. But the reflection lookup 
								// wouldn't work in that case, so those would need to provide the attributes 
								// themselves via the type descriptor.
								var reflectionProp = settings.GetType().GetProperty(property.Name);
								if (reflectionProp != null && reflectionProp.CanRead && reflectionProp.CanWrite)
								{
									// The getter is the one that matches against the interface map build before.
									var getter = reflectionProp.GetGetMethod();
									defaultValue = mappedMethods
										.Where(map => map.TargetMethod == getter)
										.Select(map => map.InterfaceProperty.GetCustomAttribute<DefaultValueAttribute>())
										// We just want to grab those where we do find a default value attribute. 
										// It may happen that a property is redefined and the attribute added/removed.
										.Where(attr => attr != null)
										.Select(attr => attr.Value)
										.FirstOrDefault();
								}
							}

							// Only try to set if we did get a raw default value somehow.
							if (defaultValue != null)
								TrySetValue(settings, property, defaultValue);
						}
					}
				}
				catch (Exception e)
				{
					tracer.Error(e, Strings.SettingsManager.FailedToRead(settings.GetType()));

					if (editable != null)
						editable.CancelEdit();

					canceled = true;
					throw;
				}
				finally
				{
					if (editable != null && !canceled)
						editable.EndEdit();

					if (initializable != null)
						initializable.EndInit();
				}
			}
		}

		internal static string GetSettingsCollectionName(Type settingsType)
		{
			return settingsType.FullName.Replace('.', '\\').Replace('+', '\\');
		}

		private void InitializeDefaultValues(object settings)
		{
			var properties = from property in TypeDescriptor.GetProperties(settings).Cast<PropertyDescriptor>()
							 let defaultValue = property.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault()
							 where defaultValue != null && defaultValue.Value != null
							 select new { Property = property, DefaultValue = defaultValue.Value };

			foreach (var property in properties)
			{
				TrySetValue(settings, property.Property, property.DefaultValue);
			}
		}

        private bool ShouldSaveValue(PropertyDescriptor property, object value, bool saveDefaults = false)
		{
            if (property.IsReadOnly || 
                property.SerializationVisibility == DesignerSerializationVisibility.Hidden)
                return false;

            // This overrides the default value probing below.
            if (saveDefaults)
                return true;

			var defaultValue = property.Attributes.OfType<DefaultValueAttribute>().Select(x => x.Value).FirstOrDefault();
			if (defaultValue != null &&
				!property.PropertyType.IsAssignableFrom(defaultValue.GetType()) &&
				property.Converter.CanConvertFrom(defaultValue.GetType()))
			{
				defaultValue = property.Converter.ConvertFrom(defaultValue);
			}

			if (value.Equals(defaultValue))
				return false;

			if (value.GetType().IsValueType)
			{
				var defaultValueTypeValue = Activator.CreateInstance(value.GetType());
				if (value.Equals(defaultValueTypeValue) && value.Equals(defaultValue))
					return false;
			}

			return true;
		}

		private static void TrySetValue<TSettings>(TSettings settings, PropertyDescriptor property, object value)
		{
			if (property.PropertyType.IsAssignableFrom(value.GetType()))
			{
				property.SetValue(settings, value);
			}
			else if (property.Converter.CanConvertFrom(value.GetType()))
			{
				property.SetValue(settings, property.Converter.ConvertFrom(value));
			}
			else
			{
				tracer.Warn(Strings.SettingsManager.InvalidValue(
                    value.GetType(),
                    typeof(TSettings).Name, property.Name, property.PropertyType.Name));
			}
		}

		private class ShellSettingsStore : ISettingsStore
		{
			private WritableSettingsStore store;

			public ShellSettingsStore(IServiceProvider serviceProvider)
			{
				var manager = new ShellSettingsManager(serviceProvider);

				this.store = manager.GetWritableSettingsStore(SettingsScope.UserSettings);
			}

			public bool CollectionExists(string collectionName)
			{
				return this.store.CollectionExists(collectionName);
			}

			public void DeleteCollection(string collectionName)
			{
				this.store.DeleteCollection(collectionName);
			}

			public void CreateCollection(string collectionName)
			{
				this.store.CreateCollection(collectionName);
			}

			public bool PropertyExists(string collectionPath, string propertyName)
			{
				return this.store.PropertyExists(collectionPath, propertyName);
			}

			public void DeleteProperty(string collectionPath, string propertyName)
			{
				this.store.DeleteProperty(collectionPath, propertyName);
			}

			public void SetString(string collectionPath, string propertyName, string stringValue)
			{
				this.store.SetString(collectionPath, propertyName, stringValue);
			}

			public string GetString(string collectionPath, string propertyName)
			{
				return this.store.GetString(collectionPath, propertyName);
			}
		}
	}
}
