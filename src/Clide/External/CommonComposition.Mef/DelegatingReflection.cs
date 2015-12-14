namespace Clide.CommonComposition
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Provides a base <see cref="ConstructorInfo"/> that can be 
    /// customized to expose tailored metadata for a constructor 
    /// to APIs that use reflection.
    /// </summary>
    internal class DelegatingConstructorInfo : ConstructorInfo
    {
        private ConstructorInfo constructor;

        public DelegatingConstructorInfo(ConstructorInfo constructor)
        {
            this.constructor = constructor;
        }

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return constructor.Invoke(invokeAttr, binder, parameters, culture);
        }

        public override MethodAttributes Attributes
        {
            get { return constructor.Attributes; }
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return constructor.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            return constructor.GetParameters();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return constructor.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { return constructor.MethodHandle; }
        }

        public override Type DeclaringType
        {
            get { return constructor.DeclaringType; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return constructor.GetCustomAttributes(attributeType, inherit);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return constructor.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return constructor.IsDefined(attributeType, inherit);
        }

        public override string Name
        {
            get { return constructor.Name; }
        }

        public override Type ReflectedType
        {
            get { return constructor.ReflectedType; }
        }
    }

    /// <summary>
    /// Provides a base <see cref="ParameterInfo"/> that can be 
    /// customized to expose tailored metadata for a parameter 
    /// to APIs that use reflection.
    /// </summary>
    internal class DelegatingParameterInfo : ParameterInfo
    {
        private readonly ParameterInfo parameter;

        public DelegatingParameterInfo(ParameterInfo parameter)
        {
            this.parameter = parameter;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.parameter.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.parameter.GetCustomAttributes(attributeType, inherit);
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return this.parameter.GetCustomAttributesData();
        }

        public override Type[] GetOptionalCustomModifiers()
        {
            return this.parameter.GetOptionalCustomModifiers();
        }

        public override Type[] GetRequiredCustomModifiers()
        {
            return this.parameter.GetRequiredCustomModifiers();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.parameter.IsDefined(attributeType, inherit);
        }

        public override string ToString()
        {
            return this.parameter.ToString();
        }

        public override ParameterAttributes Attributes
        {
            get { return this.parameter.Attributes; }
        }

        public override object DefaultValue
        {
            get { return this.parameter.DefaultValue; }
        }

        public override MemberInfo Member
        {
            get { return this.parameter.Member; }
        }

        public override int MetadataToken
        {
            get { return this.parameter.MetadataToken; }
        }

        public override string Name
        {
            get { return this.parameter.Name; }
        }

        public override Type ParameterType
        {
            get { return this.parameter.ParameterType; }
        }

        public override int Position
        {
            get { return this.parameter.Position; }
        }

        public override object RawDefaultValue
        {
            get { return this.parameter.RawDefaultValue; }
        }

        public ParameterInfo UnderlyingParameter
        {
            get { return this.parameter; }
        }
    }
}
