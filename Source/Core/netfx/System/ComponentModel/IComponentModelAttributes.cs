using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

/// <summary>
/// Provides strong typed access to ComponentModel attributes.
/// </summary>
///	<nuget id="netfx-System.ComponentModel.Attributes" />
internal partial interface IComponentModelAttributes
{
	/// <summary>
	/// Gets the value of <see cref="AmbientValueAttribute.Value"/> if present.
	/// </summary>
	object AmbientValue { get; }

	/// <summary>
	/// Gets the <see cref="AttributeProviderAttribute"/> if present.
	/// </summary>
	AttributeProviderAttribute AttributeProvider { get; }

	/// <summary>
	/// Gets the value of <see cref="BindableAttribute.Bindable"/> if present.
	/// </summary>
	bool? Bindable { get; }

	/// <summary>
	/// Gets the value of <see cref="BindableAttribute.Direction"/> if present.
	/// </summary>
	BindingDirection? BindingDirection { get; }

	/// <summary>
	/// Gets the value of <see cref="BrowsableAttribute.Browsable"/> if present.
	/// </summary>
	bool? Browsable { get; }

	/// <summary>
	/// Gets the value of <see cref="CategoryAttribute.Category"/> if present.
	/// </summary>
	string Category { get; }

	/// <summary>
	/// Gets the value of <see cref="ComplexBindingPropertiesAttribute.DataMember"/> if present.
	/// </summary>
	string ComplexBindingDataMember { get; }

	/// <summary>
	/// Gets the value of <see cref="ComplexBindingPropertiesAttribute.DataSource"/> if present.
	/// </summary>
	string ComplexBindingDataSource { get; }

	/// <summary>
	/// Gets the value of <see cref="DefaultValueAttribute.Value"/> if present.
	/// </summary>
	object DefaultValue { get; }

	/// <summary>
	/// Gets the value of <see cref="DescriptionAttribute.Description"/> if present.
	/// </summary>
	string Description { get; }

	///// <summary>
	///// Gets the value of <see cref="DesignerSerializationVisibilityAttribute.Visibility"/> if present.
	///// </summary>
	//DesignerSerializationVisibility? DesignerSerializationVisibility { get; }

	/// <summary>
	/// Gets the value of <see cref="DisplayNameAttribute.DisplayName"/> if present.
	/// </summary>
	string DisplayName { get; }


	/// <summary>
	/// Gets the value of <see cref="EditorBrowsableAttribute.State"/> if present.
	/// </summary>
	EditorBrowsableState? EditorBrowsable { get; }

	/// <summary>
	/// Gets the value of <see cref="DesignOnlyAttribute.IsDesignOnly"/> if present.
	/// </summary>
	bool? IsDesignOnly { get; }

	/// <summary>
	/// Gets the value of <see cref="ImmutableObjectAttribute.Immutable"/> if present.
	/// </summary>
	bool? IsImmutable { get; }

	/// <summary>
	/// Gets the value of <see cref="LocalizableAttribute.IsLocalizable"/> if present.
	/// </summary>
	bool? IsLocalizable { get; }

	/// <summary>
	/// Gets the value of <see cref="ReadOnlyAttribute.IsReadOnly"/> if present.
	/// </summary>
	bool? IsReadOnly { get; }

	/// <summary>
	/// Gets the value of <see cref="SettingsBindableAttribute.Bindable"/> if present.
	/// </summary>
	bool? SettingsBindable { get; }
}