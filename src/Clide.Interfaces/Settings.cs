namespace Clide
{
	using Clide.Properties;
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Diagnostics;

	/// <summary>
	/// Helper base class that can be used to provide transparent loading and saving of settings. 
	/// </summary>
	/// <remarks>
	/// Derived classes typically expose an interface that is exported to the composition container, 
	/// and declares an importing constructor that receives the settings manager, which is already 
	/// exported in the environment by the runtime.
	/// </remarks>
	/// <example>
	/// The following is an example of a settings class:
	/// <code>
	/// [Settings]
	/// public class ServerSettings : Settings, IServerSettings
	/// {
	///     public ServerSettings(ISettingsManager manager)
	///         : base(manager)
	///     {
	///     }
	///     
	///     public string Name { get; set; }
	///     public int Port { get; set; }
	/// }
	/// </code>
	/// Note how the class specifies what is the exported settings interface 
	/// for other consuming code. Also, the imported settings manager is passed 
	/// to the base class which takes care of reading and saving the state as 
	/// necessary.
	/// </example>
	public abstract class Settings : ISettings, INotifyPropertyChanged, ISupportInitialize, ISupportInitializeNotification
    {
        private ITracer tracer;

        /// <summary>
        /// Occurs when initialization of the component is completed.
        /// </summary>
        public event EventHandler Initialized = (sender, args) => { };

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };

        private bool editing;
        private bool initializing;
        private ISettingsManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="manager">The settings manager that will read and save data for this instance.</param>
        public Settings(ISettingsManager manager)
        {
            tracer = Tracer.Get(GetType());
            this.manager = manager;
            this.manager.Read(this);
            IsInitialized = false;
        }

        /// <summary>
        /// Gets a value indicating whether the component is initialized.
        /// </summary>
        /// <returns>true to indicate the component has completed initialization; otherwise, false. </returns>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Begins an edit on an object.
        /// </summary>
        public virtual void BeginEdit()
        {
            tracer.Verbose("BeginEdit");
            editing = true;
        }

        /// <summary>
        /// Discards changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit" /> call.
        /// </summary>
        public virtual void CancelEdit()
        {
            tracer.Verbose("CancelEdit");

            if (editing)
            {
                editing = false;
                // Restore a clean copy of the object, as if it was brand-new created.
                try
                {
                    var clean = Activator.CreateInstance(GetType(), manager);
                    foreach (var property in TypeDescriptor.GetProperties(this).Cast<PropertyDescriptor>())
                    {
                        property.SetValue(this, property.GetValue(clean));
                    }

                    IsInitialized = false;
                    manager.Read(this);
                }
                catch (Exception ex)
                {
                    tracer.Error(ex, Strings.Settings.FailedToRestore);
                    // TODO: failed to restore some properties. Leave current object state as-is?
                }
            }
        }

        /// <summary>
        /// Pushes changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit" /> or <see cref="M:System.ComponentModel.IBindingList.AddNew" /> call into the underlying object.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        public virtual void EndEdit()
        {
            tracer.Verbose("EndEdit");

            if (!editing)
                throw new InvalidOperationException(Strings.Settings.EndEditWithoutBeginEdit);

            if (!initializing)
                Save();

            editing = false;
        }

        /// <summary>
        /// Signals the object that initialization is starting.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        public virtual void BeginInit()
        {
            if (IsInitialized)
                throw new InvalidOperationException(Strings.Settings.AlreadyInitialized);

            initializing = true;
        }

        /// <summary>
        /// Signals the object that initialization is complete.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        public virtual void EndInit()
        {
            if (!initializing)
                throw new InvalidOperationException(Strings.Settings.EndInitWithoutBeginInit);

            IsInitialized = true;
            initializing = false;

            Initialized(this, EventArgs.Empty);
        }

        /// <summary>
        /// Saves the current settings class, optionally specifying whether to 
        /// forcedly persist values which have their defaults only.
        /// </summary>
        public virtual void Save(bool saveDefaults = false)
        {
            OnSaving();
            manager.Save(this, saveDefaults);
            OnSaved();

            tracer.Info(Strings.Settings.TraceSaved);
        }

        /// <summary>
        /// Called before saving this instance to the settings store.
        /// </summary>
        protected virtual void OnSaving()
        {
        }

        /// <summary>
        /// Called after saving this instance to the settings store.
        /// </summary>
        protected virtual void OnSaved()
        {
        }

        /// <summary>
        /// Manually raises the property changed. Not needed if using automatic 
        /// properties together with the Property Changed nuget package
        /// (https://www.nuget.org/packages/PropertyChanged.Fody/)
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
