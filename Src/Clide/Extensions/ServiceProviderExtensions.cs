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

namespace System
{
    using System.Linq;
    using System.Runtime.InteropServices;
    using Clide.Properties;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Clide;

    /// <summary>
    /// Provides useful extensions to the IDE service provider.
    /// </summary>
    public static partial class ServiceProviderExtensions
    {
        /// <summary>
        /// Gets the package GUID or throws an <see cref="ArgumentException"/> if the 
        /// <see cref="GuidAttribute"/> is not found on the given instance type.
        /// </summary>
        internal static Guid GetPackageGuidOrThrow(this IServiceProvider owningPackage)
        {
            var guid = owningPackage.GetType().GetCustomAttributes(typeof(GuidAttribute), true)
                .OfType<GuidAttribute>()
                .FirstOrDefault();

            if (guid == null)
                throw new ArgumentException(Strings.General.MissingGuidAttribute(owningPackage.GetType()));

            return new Guid(guid.Value);
        }

        /// <summary>
        /// Retrieves an existing loaded package or loads it 
        /// automatically if needed.
        /// </summary>
        /// <typeparam name="TPackage">The type of the package to load.</typeparam>
        /// <returns>The fully loaded and initialized package.</returns>
        public static TPackage GetLoadedPackage<TPackage>(this IServiceProvider serviceProvider)
        {
            return UIThread.Default.Invoke(() =>
            {
                var guidString = typeof(TPackage)
                    .GetCustomAttributes(true)
                    .OfType<GuidAttribute>()
                    .Select(g => g.Value)
                    .FirstOrDefault();

                if (guidString == null)
                    throw new InvalidOperationException(Strings.IServiceProviderExtensions.MissingGuidAttribute(typeof(TPackage)));

                var guid = new Guid(guidString);
                var vsPackage = default(IVsPackage);

                var vsShell = serviceProvider.GetService<SVsShell, IVsShell>();
                vsShell.IsPackageLoaded(ref guid, out vsPackage);

                if (vsPackage == null)
                    ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref guid, out vsPackage));

                return (TPackage)vsPackage;
            });
        }

        /// <summary>
        /// Retrieves an existing loaded package or loads it 
        /// automatically if needed.
        /// </summary>
        /// <typeparam name="TPackage">The type of the package to load.</typeparam>
        /// <returns>The fully loaded and initialized package.</returns>
        public static IServiceProvider GetLoadedPackage(this IServiceProvider serviceProvider, Guid packageId)
        {
            return UIThread.Default.Invoke(() =>
            {
                var vsPackage = default(IVsPackage);

                var vsShell = serviceProvider.GetService<SVsShell, IVsShell>();
                vsShell.IsPackageLoaded(ref packageId, out vsPackage);

                if (vsPackage == null)
                    ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref packageId, out vsPackage));

                return (IServiceProvider)vsPackage;
            });
        }
    }
}
