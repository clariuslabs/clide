namespace Clide.CommonComposition
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A custom MEF catalog that provides the behavior of automatically exposing 
    /// the annotated components from the assemblies or types received 
    /// in the constructor.
    /// </summary>
    /// <remarks>
    /// <example>
    /// The following example registers all annotated components from the given 
    /// given assembly with the container configuration:
    ///     <code>
    ///     var configuration = new ContainerConfiguration();
    ///     configuration.RegisterComponents(typeof(IFoo).Assembly);
    ///
    ///     var container = configuration.CreateContainer();
    ///     </code>
    /// </example>
    /// </remarks>
    public class ComponentCatalog : TypeCatalog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentCatalog"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for components.</param>
        public ComponentCatalog(params Assembly[] assemblies)
            : this(assemblies.SelectMany(a => a.GetTypes()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentCatalog"/> class.
        /// </summary>
        /// <param name="types">The types to scan for <see cref="ComponentAttribute"/>-annotated ones.</param>
        public ComponentCatalog(params Type[] types)
            : this((IEnumerable<Type>)types)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentCatalog"/> class.
        /// </summary>
        /// <param name="types">The types to scan for <see cref="ComponentAttribute"/>-annotated ones.</param>
        public ComponentCatalog(IEnumerable<Type> types)
            : base(types.Where(t => t.IsDefined(typeof(ComponentAttribute), true) && !t.IsAbstract).Select(t => new ComponentType(t)))
        {
        }

        private class ComponentType : TypeDelegator
        {
            private Type type;
            private Lazy<ComponentAttribute> component;
            private Lazy<object[]> additionalAttributes;
            private Lazy<PartCreationPolicyAttribute[]> creationAttributes;
            private Lazy<ExportAttribute[]> exportAttributes;
            private Lazy<string> name;

            public ComponentType(Type type)
                : base(type)
            {
                this.type = type;
                this.component = new Lazy<ComponentAttribute>(() =>
                    type.GetCustomAttributes(typeof(ComponentAttribute), true).OfType<ComponentAttribute>().First());

                this.exportAttributes = new Lazy<ExportAttribute[]>(() =>
                {
                    var exports = new List<ExportAttribute>();
                    exports.Add(new ExportAttribute(name.Value, type));
                    exports.AddRange(type
                        .GetInterfaces()
                        .Where(i => i != typeof(IDisposable))
                        .Select(i => new ExportAttribute(name.Value, i)));

                    return exports.ToArray();
                });

                this.creationAttributes = new Lazy<PartCreationPolicyAttribute[]>(() =>
                    new PartCreationPolicyAttribute[] { new PartCreationPolicyAttribute(component.Value.IsSingleton ? CreationPolicy.Shared : CreationPolicy.NonShared) });

                this.additionalAttributes = new Lazy<object[]>(() => new object[0].Concat(exportAttributes.Value).Concat(creationAttributes.Value).ToArray());

                this.name = new Lazy<string>(() => type
                    .GetCustomAttributes(typeof(NamedAttribute), true)
                    .OfType<NamedAttribute>()
                    .Select(x => x.Name)
                    .FirstOrDefault());
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                if (attributeType == typeof(ExportAttribute) ||
                    attributeType == typeof(InheritedExportAttribute) ||
                    attributeType == typeof(PartCreationPolicyAttribute))
                    return true;

                return base.IsDefined(attributeType, inherit);
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return base.GetCustomAttributes(inherit).Concat(additionalAttributes.Value).ToArray();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                if (attributeType == typeof(ExportAttribute))
                    return exportAttributes.Value;
                else if (attributeType == typeof(PartCreationPolicyAttribute))
                    return creationAttributes.Value;

                return base.GetCustomAttributes(attributeType, inherit);
            }

            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            {
                // Do not expose non-public constructors for composition, since 
                // this is the behavior of all other frameworks (including Microsoft.Composition!)
                if (bindingAttr.HasFlag(BindingFlags.NonPublic))
                    bindingAttr &= ~BindingFlags.NonPublic;

                var ctor = base.GetConstructors(bindingAttr).OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
                if (ctor != null)
                    return new ConstructorInfo[] { new ImportingConstructorInfo(ctor) };

                return new ConstructorInfo[0];
            }

            private class ImportingConstructorInfo : DelegatingConstructorInfo
            {
                private Lazy<ParameterInfo[]> parameters;

                public ImportingConstructorInfo(ConstructorInfo constructor)
                    : base(constructor)
                {
                    this.parameters = new Lazy<ParameterInfo[]>(() => constructor.GetParameters().Select(x => new ImportedParameterInfo(x)).ToArray());
                }

                public override bool IsDefined(Type attributeType, bool inherit)
                {
                    if (attributeType == typeof(ImportingConstructorAttribute))
                        return true;

                    return base.IsDefined(attributeType, inherit);
                }

                public override object[] GetCustomAttributes(Type attributeType, bool inherit)
                {
                    if (attributeType == typeof(ImportingConstructorAttribute))
                        return new ImportingConstructorAttribute[] { new ImportingConstructorAttribute() };

                    return base.GetCustomAttributes(attributeType, inherit);
                }

                public override object[] GetCustomAttributes(bool inherit)
                {
                    return base.GetCustomAttributes(inherit)
                        .Concat(new ImportingConstructorAttribute[] { new ImportingConstructorAttribute() })
                        .ToArray();
                }

                public override ParameterInfo[] GetParameters()
                {
                    return parameters.Value;
                }
            }

            private class ImportedParameterInfo : DelegatingParameterInfo
            {
                // Both import attributes implement IAttributeImport, an internal interface 
                // which is used when querying for the import or import many attributes. 
                // We don't want to hardcode that here, so we retrieve the interface type 
                // via reflection.
                private Type attributedImportInterface = typeof(ImportAttribute).GetInterfaces().First(i => i.Namespace.StartsWith("System.ComponentModel.Composition"));
                private Attribute import;

                public ImportedParameterInfo(ParameterInfo parameter)
                    : base(parameter)
                {
                    var name = parameter.GetCustomAttributes(typeof(NamedAttribute), true)
                        .OfType<NamedAttribute>()
                        .Select(x => x.Name)
                        .FirstOrDefault();

                    if (parameter.ParameterType.IsGenericType &&
                        parameter.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        this.import = new ImportManyAttribute(name);
                    else
                        this.import = new ImportAttribute(name);
                }

                public override bool IsDefined(Type attributeType, bool inherit)
                {
                    if (attributeType == attributedImportInterface)
                        return true;

                    if (attributeType == typeof(ImportManyAttribute))
                        return import is ImportManyAttribute;
                    else if (attributeType == typeof(ImportAttribute))
                        return import is ImportAttribute;
                        
                    return base.IsDefined(attributeType, inherit);
                }

                public override object[] GetCustomAttributes(Type attributeType, bool inherit)
                {
                    if (attributeType == attributedImportInterface)
                    {
                        var attrs = (object[])Array.CreateInstance(attributedImportInterface, 1);
                        attrs[0] = import;
                        return attrs;
                    }

                    return base.GetCustomAttributes(attributeType, inherit);
                }
            }
        }
    }
}