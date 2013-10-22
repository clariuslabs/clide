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

namespace Clide
{
    using Clide.Composition;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Default implementation of the reference service.
    /// </summary>
    [Component(typeof(IReferenceService), typeof(IReferenceProviderRegistry))]
    internal class ReferenceService : IReferenceService, IReferenceProviderRegistry
    {
        private Dictionary<string, IReferenceProvider> providers = new Dictionary<string, IReferenceProvider>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceService"/> class.
        /// </summary>
        public ReferenceService()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceService"/> class.
        /// </summary>
        /// <param name="providers">The registered providers.</param>
        public ReferenceService(IEnumerable<IReferenceProvider> providers)
        {
            foreach (var provider in providers)
            {
                Register(provider);
            }
        }

        /// <summary>
        /// Gets the registered providers.
        /// </summary>
        public IEnumerable<IReferenceProvider> Providers { get { return this.providers.Values; } }

        /// <summary>
        /// Registers the specified provider with the service.
        /// </summary>
        /// <param name="provider">The reference provider to register.</param>
        /// <exception cref="ArgumentException">A provider has already been registered 
        /// for the same <see cref="IReferenceProvider.Scheme"/> scheme.</exception>
        public void Register(IReferenceProvider provider)
        {
            if (IsRegistered(provider.Scheme))
                throw new ArgumentException();

            this.providers[provider.Scheme] = provider;
        }

        /// <summary>
        /// Checks if the reference scheme is registered as a valid provider.
        /// </summary>
        /// <param name="scheme">The reference scheme to be checked, such as "vsix" or "project", with or without the trailing ":" or "://".</param>
        /// <returns>
        ///   <see langword="true" />, if a reference provider exists for the scheme; otherwise <see langword="false" />
        /// </returns>
        public bool IsRegistered(string scheme)
        {
            var normalized = Normalize(scheme);

            return this.providers.ContainsKey(normalized);
        }

        /// <summary>
        /// Tries to resolve the given reference to an instance of T based on the
        /// reference scheme and the registered reference providers.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be resolved</typeparam>
        /// <param name="reference">The reference to try to resolve.</param>
        /// <returns>
        /// The resolved reference or <see langword="null" /> if it could
        /// not be resolved to a valid instance of <typeparamref name="T" />.
        /// </returns>
        /// <exception cref="System.ArgumentException"></exception>
        public T TryResolveReference<T>(string reference) where T : class
        {
            Guard.NotNullOrEmpty(() => reference, reference);

            var indexOfColon = reference.IndexOf(':');
            if (indexOfColon == -1)
                throw new ArgumentException();
            var scheme = reference.Substring(indexOfColon);

            dynamic provider = GetProvidersAssignableTo(scheme, typeof(T)).FirstOrDefault();
            if (provider != null)
                // it is not ok to cast provider to IReferenceProvider<T> this would only be valid of IReferenceProvider<T> is covariant.
                return provider.TryResolveReference(reference);

            return default(T);
        }

        /// <summary>
        /// Tries to open the instance in the appropiate view.
        /// </summary>
        /// <typeparam name="T">The type of the instance to open.</typeparam>
        /// <param name="instance">The instance to open in its default view.</param>
        /// <param name="scheme">The optional scheme to use to determine how the reference should be opened.
        /// If none is specified, a registered <see cref="IReferenceProvider{T}" /> of the given
        /// <typeparamref name="T" /> would open the default view.</param>
        /// <returns>
        ///   <see langword="true" /> if the instance could be opened; <see langword="false" /> otherwise.
        /// </returns>
        public bool TryOpen<T>(T instance, string scheme = null) where T : class
        {
            Guard.NotNull(() => instance, instance);

            var provider = GetReferenceProvider(instance, scheme);

            if (provider != null)
                return provider.TryOpen(instance);

            return false;
        }

        /// <summary>
        /// Tries to create a reference for the instance of T.
        /// </summary>
        /// <typeparam name="T">The type of the instance to create a reference for.</typeparam>
        /// <param name="instance">The instance to create a reference for.</param>
        /// <param name="scheme">The optional scheme to use to determine how the reference should be created. If
        /// none is specified, a registered <see cref="IReferenceProvider{T}" /> of the given
        /// <typeparamref name="T" /> would create the reference.</param>
        /// <returns>
        /// The reference to the instance or <see langword="null" /> if no registered
        /// provider could create the reference.
        /// </returns>
        public string TryCreateReference<T>(T instance, string scheme = null) where T : class
        {
            Guard.NotNull(() => instance, instance);

            var provider = GetReferenceProvider<T>(instance, scheme);

            if (provider != null)
                return provider.TryCreateReference(instance);

            return null;
        }

        private static string Normalize(string scheme)
        {
            var normalized = scheme;
            var indexOfColon = normalized.IndexOf(':');
            if (indexOfColon != -1)
                normalized = normalized.Substring(0, indexOfColon);
            return normalized;
        }

        private dynamic GetReferenceProvider<T>(T instance, string uriScheme) where T : class
        {
            IEnumerable<IReferenceProvider> compatibleProviders;

            var instanceType = instance.GetType();
            compatibleProviders = GetProvidersAssignableFrom(uriScheme, instanceType);

            return compatibleProviders.FirstOrDefault();
        }

        private IEnumerable<IReferenceProvider> GetProvidersAssignableFrom(string scheme, Type targetType)
        {
            var providers = this.Providers;
            if (scheme != null)
                providers = providers.Where(provider => provider.Scheme == scheme);

            var compatibleProviders = providers.Where(provider =>
                GetImplementedProviderInterfaces(provider.GetType())
                    .Any(i => i.GetGenericArguments()[0].IsAssignableFrom(targetType)));

            return compatibleProviders;
        }

        private IEnumerable<IReferenceProvider> GetProvidersAssignableTo(string scheme, Type targetType)
        {
            var providers = this.Providers;
            if (scheme != null)
                providers = providers.Where(provider => provider.Scheme == scheme);

            var compatibleProviders = providers.Where(provider =>
                 GetImplementedProviderInterfaces(provider.GetType())
                    .Any(i => targetType.IsAssignableFrom(i.GetGenericArguments()[0])));

            return compatibleProviders;
        }

        private static IEnumerable<Type> GetImplementedProviderInterfaces(Type providerType)
        {
            return providerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReferenceProvider<>));
        }
    }
}