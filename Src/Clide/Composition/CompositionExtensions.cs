#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Composition
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.ComponentModel.Composition.ReflectionModel;
    using System.Linq;
    using System.Reflection;
    using Autofac;
    using Autofac.Builder;
    using Autofac.Core;
    using Autofac.Features.Metadata;
    using Autofac.Features.Scanning;
    using Microsoft.VisualStudio.ComponentModelHost;

    /// <summary>
    /// Provides automatic component registration by scanning assemblies and types for 
    /// those that have the <see cref="ComponentAttribute"/> annotation.
    /// </summary>
    public static class CompositionExtensions
    {
        /// <summary>
        /// Registers the composition component model extensions with the builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="composition">The composition.</param>
        public static void RegisterComponentModel(this ContainerBuilder builder, IComponentModel composition)
        {
            // Register module that automatically satisfies imports on components.
            builder.RegisterModule(new CompositionModule(composition.DefaultCompositionService));
            // Register source that exposes MEF-registered types to components.
            builder.RegisterSource(new CompositionSource(composition.DefaultCatalog, composition.DefaultExportProvider));
        }

        /// <summary>
        /// Registers the service provider with the builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="services">The services.</param>
        public static void RegisterServiceProvider(this ContainerBuilder builder, IServiceProvider services)
        {
            builder.RegisterSource(new ServiceProviderSource(services));
        }

        /// <summary>
        /// Registers the components found in the given assemblies.
        /// </summary>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterAssemblyComponents(this ContainerBuilder builder, ExportProvider exports, params Assembly[] assemblies)
        {
            // Allow non-public types just like MEF does.
            return RegisterComponents(builder, exports, assemblies.SelectMany(x => x.GetTypes()));
        }

        /// <summary>
        /// Registers the components found in the given set of types.
        /// </summary>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterComponents(this ContainerBuilder builder, ExportProvider exports, params Type[] types)
        {
            return RegisterComponents(builder, exports, (IEnumerable<Type>)types);
        }

        /// <summary>
        /// Registers the components found in the given set of types.
        /// </summary>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterComponents(this ContainerBuilder builder, ExportProvider exports, IEnumerable<Type> types)
        {
            var registration = builder
                .RegisterTypes(types.Where(t => t.GetCustomAttribute<ComponentAttribute>() != null).ToArray())
                // Allow non-public constructors just like MEF does.
                .FindConstructorsWith(t => t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                .As(t =>
                {
                    var attr = t.GetCustomAttribute<ComponentAttribute>();
                    var registerKey = attr.RegisterKey;
                    if (registerKey != null)
                    {
                        // There can only be zero or one RegisterAs in this case. 
                        var registerType = attr.RegisterAs.FirstOrDefault() ?? t;
                        return new Service[] { new KeyedService(registerKey, registerType) };
                    }

                    // Otherwise, we may have any number of registration types, or none at all.
                    if (attr.RegisterAs.Length == 0)
                        return new Service[] { new TypedService(t) };

                    return attr.RegisterAs.Select(reg => new TypedService(reg));
                })
                .WithImports(exports)
                .WithKeyFilter()
                .PropertiesAutowired(PropertyWiringOptions.PreserveSetValues)
                .WithMetadataFilter();

            // Optionally set the SingleInstance behavior.
            registration.ActivatorData.ConfigurationActions.Add((t, rb) =>
            {
                if (rb.ActivatorData.ImplementationType.GetCustomAttribute<ComponentAttribute>().IsSingleton)
                    rb.SingleInstance();
            });

            return registration;
        }

        /// <summary>
        /// Specifies that the registered components may have MEF imports as 
        /// constructor parameters.
        /// </summary>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle>
            WithImports<TLimit, TReflectionActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration,
                ExportProvider exports)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            return registration
                .WithParameter(
                    (p, c) => p.GetCustomAttributes(true).OfType<ImportAttribute>().Any(),
                    (p, c) =>
                    {
                        var import = p.GetCustomAttributes(true).OfType<ImportAttribute>().First();
                        var contractName = import.ContractName ??
                            AttributedModelServices.GetContractName(import.ContractType ?? GetElementType(p.ParameterType));
                        var contractType = import.ContractType ?? GetElementType(p.ParameterType);

                        var export = exports.GetExports(contractType, typeof(IDictionary<string, object>), contractName)
                            .FirstOrDefault();

                        if (export != null)
                            return export.Value;
                        else if (!import.AllowDefault)
                            throw new ImportCardinalityMismatchException(String.Format("Could not satisfy import of {0} on parameter {1}(...{2} {3}...).", 
                                contractName, p.Member.DeclaringType.Name, p.ParameterType.Name, p.Name));

                        return null;
                    })
                .WithParameter(
                    (p, c) => p.GetCustomAttributes(true).OfType<ImportManyAttribute>().Any(),
                    (p, c) => { throw new NotSupportedException("[ImportMany] is not supported."); });
        }

        private static Type GetElementType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            return type;
        }
    }
}