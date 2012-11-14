#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

/// <summary>
/// Provides strong typed access to ComponentModel attributes by using the ComponentModel() extension method over a type, method, property, etc.
/// </summary>
///	<nuget id="netfx-System.ComponentModel.Attributes" />
internal static partial class ComponentModelAttributes
{
	/// <summary>
	///	Provides strong typed access to System.ComponentModel attributes for a type, method, property, etc.
	/// </summary>
	/// <param name="reflectionObject" this="true">The object this extension method applies to.</param>
	/// <param name="inherit">Whether to retrieve attributes from base types.</param>
	public static IComponentModelAttributes ComponentModel(this ICustomAttributeProvider reflectionObject, bool inherit = true)
	{
		Guard.NotNull(() => reflectionObject, reflectionObject);

		return new Attributes(reflectionObject);
	}

	private class Attributes : IComponentModelAttributes
	{
		private ICustomAttributeProvider target;
		private bool inherit;

		public Attributes(ICustomAttributeProvider reflectionObject, bool inherit = true)
		{
			this.target = reflectionObject;
			this.inherit = inherit;
		}

		public object AmbientValue
		{
			get { return this.target.GetCustomAttributes<AmbientValueAttribute>(this.inherit).Select(x => x.Value).FirstOrDefault(); }
		}

		public AttributeProviderAttribute AttributeProvider
		{
			get { return this.target.GetCustomAttribute<AttributeProviderAttribute>(this.inherit); }
		}

		public bool? Bindable
		{
			get { return this.target.GetCustomAttributes<BindableAttribute>(this.inherit).Select(x => (bool?)x.Bindable).FirstOrDefault(); }
		}

		public BindingDirection? BindingDirection
		{
			get { return this.target.GetCustomAttributes<BindableAttribute>(this.inherit).Select(x => (BindingDirection?)x.Direction).FirstOrDefault(); }
		}

		public bool? Browsable
		{
			get { return this.target.GetCustomAttributes<BrowsableAttribute>(this.inherit).Select(x => (bool?)x.Browsable).FirstOrDefault(); }
		}

		public string Category
		{
			get { return this.target.GetCustomAttributes<CategoryAttribute>(this.inherit).Select(x => x.Category).FirstOrDefault(); }
		}

		public string ComplexBindingDataMember
		{
			get { return this.target.GetCustomAttributes<ComplexBindingPropertiesAttribute>(this.inherit).Select(x => x.DataMember).FirstOrDefault(); }
		}

		public string ComplexBindingDataSource
		{
			get { return this.target.GetCustomAttributes<ComplexBindingPropertiesAttribute>(this.inherit).Select(x => x.DataSource).FirstOrDefault(); }
		}

		public object DefaultValue
		{
			get { return this.target.GetCustomAttributes<DefaultValueAttribute>(this.inherit).Select(x => x.Value).FirstOrDefault(); }
		}

		public string Description
		{
			get { return this.target.GetCustomAttributes<DescriptionAttribute>(this.inherit).Select(x => x.Description).FirstOrDefault(); }
		}

		//public DesignerSerializationVisibility? DesignerSerializationVisibility
		//{
		//    get { return this.target.GetCustomAttributes<DesignerSerializationVisibilityAttribute>(this.inherit).Select(x => (DesignerSerializationVisibility?)x.Visibility).FirstOrDefault(); }
		//}

		public string DisplayName
		{
			get { return this.target.GetCustomAttributes<DisplayNameAttribute>(this.inherit).Select(x => x.DisplayName).FirstOrDefault(); }
		}

		public EditorBrowsableState? EditorBrowsable
		{
			get { return this.target.GetCustomAttributes<EditorBrowsableAttribute>(this.inherit).Select(x => (EditorBrowsableState?)x.State).FirstOrDefault(); }
		}

		public bool? IsDesignOnly
		{
			get { return this.target.GetCustomAttributes<DesignOnlyAttribute>(this.inherit).Select(x => (bool?)x.IsDesignOnly).FirstOrDefault(); }
		}

		public bool? IsImmutable
		{
			get { return this.target.GetCustomAttributes<ImmutableObjectAttribute>(this.inherit).Select(x => (bool?)x.Immutable).FirstOrDefault(); }
		}

		public bool? IsLocalizable
		{
			get { return this.target.GetCustomAttributes<LocalizableAttribute>(this.inherit).Select(x => (bool?)x.IsLocalizable).FirstOrDefault(); }
		}

		public bool? IsReadOnly
		{
			get { return this.target.GetCustomAttributes<ReadOnlyAttribute>(this.inherit).Select(x => (bool?)x.IsReadOnly).FirstOrDefault(); }
		}

		public bool? SettingsBindable
		{
			get { return this.target.GetCustomAttributes<SettingsBindableAttribute>(this.inherit).Select(x => (bool?)x.Bindable).FirstOrDefault(); }
		}
	}
}