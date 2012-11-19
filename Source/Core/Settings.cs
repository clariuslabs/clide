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
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Clide.Properties;

namespace Clide
{
	/// <summary>
	/// Helper base class that can be used to provide transparent loading and saving of settings. 
	/// This class is already annotated with the <see cref="SettingsAttribute"/> so 
	/// derived classes don't need to.
	/// </summary>
	/// <remarks>
	/// Derived classes typically expose an interface that is exported to the composition container, 
	/// and declares an importing constructor that receives the settings manager, which is already 
	/// exported in the environment by the runtime.
	/// </remarks>
	public abstract class Settings : ISettings, INotifyPropertyChanged, ISupportInitialize, ISupportInitializeNotification
	{
		public event EventHandler Initialized = (sender, args) => { };
		public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };

		private bool editing;
		private bool initializing;
		private ISettingsManager manager;

		public Settings(ISettingsManager manager)
		{
			this.manager = manager;
			this.manager.Read(this);
			this.IsInitialized = false;
		}

		public bool IsInitialized { get; private set; }

		public virtual void BeginEdit()
		{
			this.editing = true;
		}

		public virtual void CancelEdit()
		{
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
						this.PropertyChanged(this, new PropertyChangedEventArgs(property.Name));
					}
				}
				catch (Exception)
				{
					// TODO: failed to restore some properties. Leave current object state as-is?
				}
			}
		}

		public virtual void EndEdit()
		{
			if (!this.editing)
				throw new InvalidOperationException(Strings.Settings.EndEditWithoutBeginEdit);

			if (!this.initializing)
				this.SaveChanges();

			this.editing = false;
		}

		public virtual void BeginInit()
		{
			if (this.IsInitialized)
				throw new InvalidOperationException(Strings.Settings.AlreadyInitialized);

			this.initializing = true;
		}

		public virtual void EndInit()
		{
			if (!this.initializing)
				throw new InvalidOperationException(Strings.Settings.EndInitWithoutBeginInit);

			this.IsInitialized = true;
			this.initializing = false;

			this.Initialized(this, EventArgs.Empty);
		}

		protected virtual void SaveChanges()
		{
			this.manager.Save(this);
			OnSaveChanges();
		}

		protected virtual void OnSaveChanges()
		{
		}

		protected static void RaiseChanged<TSource, TProperty>(TSource @this, Expression<Func<TSource, TProperty>> property)
			where TSource : Settings
		{
			@this.RaiseChangedImpl(property);
		}

		private void RaiseChangedImpl<TSource, TProperty>(Expression<Func<TSource, TProperty>> property)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(Reflect<TSource>.GetPropertyName(property)));
			if (!this.editing && !this.initializing)
				this.SaveChanges();
		}
	}
}
