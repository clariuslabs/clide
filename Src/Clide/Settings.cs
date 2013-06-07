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
    using System.Linq;
    using System.Linq.Expressions;
    using Clide.Properties;
    using System.Diagnostics;
    using Clide.Diagnostics;

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
    /// [Settings(typeof(IServerSettings))]
    /// public class ServerSettings : Settings, IServerSettings
    /// {
    ///     public FooSettings(ISettingsManager manager)
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
        public event EventHandler Initialized = (sender, args) => { };
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
            this.tracer = Tracer.Get(this.GetType());
            this.manager = manager;
            this.manager.Read(this);
            this.IsInitialized = false;
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
            this.editing = true;
        }

        /// <summary>
        /// Discards changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit" /> call.
        /// </summary>
        public virtual void CancelEdit()
        {
            tracer.Verbose("CancelEdit");

            if (this.editing)
            {
                this.editing = false;
                // Restore a clean copy of the object, as if it was brand-new created.
                try
                {
                    var clean = Activator.CreateInstance(this.GetType(), this.manager);
                    foreach (var property in TypeDescriptor.GetProperties(this).Cast<PropertyDescriptor>())
                    {
                        property.SetValue(this, property.GetValue(clean));
                    }

                    this.IsInitialized = false;
                    this.manager.Read(this);
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

            if (!this.editing)
                throw new InvalidOperationException(Strings.Settings.EndEditWithoutBeginEdit);

            if (!this.initializing)
                this.Save();

            this.editing = false;
        }

        /// <summary>
        /// Signals the object that initialization is starting.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        public virtual void BeginInit()
        {
            if (this.IsInitialized)
                throw new InvalidOperationException(Strings.Settings.AlreadyInitialized);

            this.initializing = true;
        }

        /// <summary>
        /// Signals the object that initialization is complete.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        public virtual void EndInit()
        {
            if (!this.initializing)
                throw new InvalidOperationException(Strings.Settings.EndInitWithoutBeginInit);

            this.IsInitialized = true;
            this.initializing = false;

            this.Initialized(this, EventArgs.Empty);
        }

        /// <summary>
        /// Saves the current settings class, optionally specifying whether to 
        /// forcedly persist values which have their defaults only.
        /// </summary>
        public virtual void Save(bool saveDefaults = false)
        {
            OnSaving();
            this.manager.Save(this, saveDefaults);
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
    }
}
