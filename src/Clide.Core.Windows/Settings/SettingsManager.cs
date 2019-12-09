﻿using Clide.Properties;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
namespace Clide
{

    /// <summary>
    /// Default implementation of <see cref="ISettingsManager"/> which uses <see cref="ShellSettingsManager"/>.
    /// </summary>
    [Export(typeof(ISettingsManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class SettingsManager : ISettingsManager
    {
        private static readonly ITracer tracer = Tracer.Get<SettingsManager>();

        private Lazy<ISettingsStore> settingsStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsManager"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        [ImportingConstructor]
        public SettingsManager([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : this(serviceProvider, new Lazy<ISettingsStore>(() => new ShellSettingsStore(serviceProvider)))
        { }

        internal SettingsManager(IServiceProvider serviceProvider, Lazy<ISettingsStore> settingsStore)
        {
            this.ServiceProvider = serviceProvider;
            this.settingsStore = settingsStore;
        }

        internal IServiceProvider ServiceProvider { get; set; }

        public void Save(object settings, bool saveDefaults = false)
        {
            // Always validates 
            Validator.ValidateObject(settings, new ValidationContext(settings, this.ServiceProvider, null));

            var store = settingsStore.Value;
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
                        if (!ShouldSaveValue(property, value, saveDefaults))
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
            var settingsType = settings.GetType();
            var store = settingsStore.Value;
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

                store = manager.GetWritableSettingsStore(SettingsScope.UserSettings);
            }

            public bool CollectionExists(string collectionName)
            {
                return store.CollectionExists(collectionName);
            }

            public void DeleteCollection(string collectionName)
            {
                store.DeleteCollection(collectionName);
            }

            public void CreateCollection(string collectionName)
            {
                store.CreateCollection(collectionName);
            }

            public bool PropertyExists(string collectionPath, string propertyName)
            {
                return store.PropertyExists(collectionPath, propertyName);
            }

            public void DeleteProperty(string collectionPath, string propertyName)
            {
                store.DeleteProperty(collectionPath, propertyName);
            }

            public void SetString(string collectionPath, string propertyName, string stringValue)
            {
                store.SetString(collectionPath, propertyName, stringValue);
            }

            public string GetString(string collectionPath, string propertyName)
            {
                return store.GetString(collectionPath, propertyName);
            }
        }
    }
}
