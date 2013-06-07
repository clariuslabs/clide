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
    using System.Linq;
    using System.Reflection;
    using Autofac;
    using Autofac.Builder;
    using Autofac.Features.Metadata;

    internal static class AutofacAttributeExtensions
    {
        /// <summary>
        /// Reference to the <see cref="Autofac.Extras.Attributed.AutofacAttributeExtensions.FilterOne{T}"/>
        /// method used in creating a closed generic reference during registration.
        /// </summary>
        private static readonly MethodInfo filterOne = typeof(AutofacAttributeExtensions).GetMethod("FilterOne", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

        /// <summary>
        /// Reference to the <see cref="Autofac.Extras.Attributed.AutofacAttributeExtensions.FilterAll{T}"/>
        /// method used in creating a closed generic reference during registration.
        /// </summary>
        private static readonly MethodInfo filterAll = typeof(AutofacAttributeExtensions).GetMethod("FilterAll", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod);


        /// <summary>
        /// Applies metadata filtering on constructor dependencies for use with the
        /// <see cref="Autofac.Extras.Attributed.WithMetadataAttribute"/>.
        /// </summary>
        /// <typeparam name="TLimit">The type of the registration limit.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style type.</typeparam>
        /// <param name="builder">The registration builder containing registration data.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Apply this metadata filter to component registrations that use the
        /// <see cref="Autofac.Extras.Attributed.WithMetadataAttribute"/> in their constructors.
        /// Doing so will allow the metadata filtering to occur. See
        /// <see cref="Autofac.Extras.Attributed.WithMetadataAttribute"/> for an
        /// example on how to use the filter and attribute together.
        /// </para>
        /// </remarks>
        /// <seealso cref="Autofac.Extras.Attributed.WithMetadataAttribute"/>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TRegistrationStyle>
            WithMetadataFilter<TLimit, TReflectionActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TRegistrationStyle> builder)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            return builder.WithParameter(
                (p, c) => p.GetCustomAttributes(true).OfType<WithMetadataAttribute>().Any(),
                (p, c) =>
                {
                    var filter = p.GetCustomAttributes(true).OfType<WithMetadataAttribute>().First();

                    // GetElementType currently is the effective equivalent of "Determine if the type
                    // is in IEnumerable and if it is, get the type being enumerated." This doesn't support
                    // the other relationship types like Lazy<T>, Func<T>, etc. If we need to add that,
                    // this is the place to do it.
                    var elementType = GetElementType(p.ParameterType);
                    var hasMany = elementType != p.ParameterType;

                    if (hasMany)
                    {
                        return filterAll.MakeGenericMethod(elementType).Invoke(null, new object[] { c, filter });
                    }

                    return filterOne.MakeGenericMethod(elementType).Invoke(null, new object[] { c, filter });
                });
        }

        /// <summary>
        /// Applies key filtering on constructor dependencies for use with the
        /// <see cref="Autofac.Extras.Attributed.WithKeyAttribute"/>.
        /// </summary>
        /// <typeparam name="TLimit">The type of the registration limit.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style type.</typeparam>
        /// <param name="builder">The registration builder containing registration data.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Apply this key filter to component registrations that use the
        /// <see cref="Autofac.Extras.Attributed.WithKeyAttribute"/> in their constructors.
        /// Doing so will allow the key filtering to occur. See
        /// <see cref="Autofac.Extras.Attributed.WithKeyAttribute"/> for an
        /// example on how to use the filter and attribute together.
        /// </para>
        /// </remarks>
        /// <seealso cref="Autofac.Extras.Attributed.WithKeyAttribute"/>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TRegistrationStyle>
            WithKeyFilter<TLimit, TReflectionActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TRegistrationStyle> builder)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            return builder.WithParameter(
                (p, c) => p.GetCustomAttributes(true).OfType<WithKeyAttribute>().Any(),
                (p, c) =>
                {
                    var attr = p.GetCustomAttributes(typeof(WithKeyAttribute), true).OfType<WithKeyAttribute>().First();
                    object value;
                    c.TryResolveKeyed(attr.Key, p.ParameterType, out value);
                    return value;
                });
        }

        private static Type GetElementType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            return type;
        }

        private static T FilterOne<T>(IComponentContext context, WithMetadataAttribute filter)
        {
            // Using Lazy<T> to ensure components that aren't actually used won't get activated.
            return context.Resolve<IEnumerable<Meta<Lazy<T>>>>()
                .Where(m => m.Metadata.ContainsKey(filter.Key) && filter.Value.Equals(m.Metadata[filter.Key]))
                .Select(m => m.Value.Value)
                .FirstOrDefault();
        }

        private static IEnumerable<T> FilterAll<T>(IComponentContext context, WithMetadataAttribute filter)
        {
            // Using Lazy<T> to ensure components that aren't actually used won't get activated.
            return context.Resolve<IEnumerable<Meta<Lazy<T>>>>()
                .Where(m => m.Metadata.ContainsKey(filter.Key) && filter.Value.Equals(m.Metadata[filter.Key]))
                .Select(m => m.Value.Value)
                .ToArray();
        }
    }
}