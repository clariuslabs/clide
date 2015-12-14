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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Resolves assemblies that are referenced from the calling assembly by 
    /// looking them up in a specified local directory. This enables side-by-side 
    /// versioning of locally deployed signed assemblies such as Clide itself.
    /// </summary>
    /// <remarks>
    /// This is required in order to optimize the assembly loading behavior 
    /// for scenarios where multiple plugins or extensions may deploy 
    /// different versions of the same utility assembly. 
    /// <para>
    /// This resolver ensures that assemblies that are resolved in the 
    /// <see cref="AppDomain"/> for versions that match the ones deployed 
    /// in the specified local directory, are loaded from it. 
    /// </para>
    /// <para>
    /// This means that if an older assembly is already loaded in the 
    /// <see cref="AppDomain"/>, instead of failing at runtime because 
    /// of the vesion mismatch (default CLR behavior), the new version 
    /// deployed in the local directory will be loaded instead, allowing 
    /// both extensions to coexist even if they use different versions 
    /// of the same assembly.
    /// </para>
    /// <para>
    /// The combined behavior of the CLR and this local resolver guarantees 
    /// that each extension will get at least the same version that it 
    /// was compiled and deployed with, and at most the latest version 
    /// deployed by any extension. As long as the library is backwards 
    /// compatible, all extensions will happily coexist.
    /// </para>
    /// </remarks>
    public static class LocalResolver
    {
        private static Lazy<Dictionary<string, string>> localAssemblyNames;

        /// <summary>
        /// Initializes the resolver to lookup assemblies from the 
        /// specified local directory.
        /// </summary>
        /// <param name="localDirectory">The local directory to add to the 
        /// assembly resolve probing.</param>
        public static void Initialize(string localDirectory)
        {
            localAssemblyNames = new Lazy<Dictionary<string, string>>(() => LoadAssemblyNames(localDirectory));

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                // NOTE: since we load our full names only in the local assembly set, 
                // we will only return our assembly version if it matches exactly the 
                // full name of the received arguments.
				if (localAssemblyNames.Value.ContainsKey (args.Name)) 
					return Assembly.LoadFrom(localAssemblyNames.Value[args.Name]);

                return null;
            };
        }

        private static Dictionary<string, string> LoadAssemblyNames(string localDirectory)
        {
            var names = new Dictionary<string, string>();
            foreach (var file in Directory.EnumerateFiles(localDirectory, "*.dll"))
            {
                try
                {
                    names.Add(AssemblyName.GetAssemblyName(file).FullName, file);
                }
                catch (System.Security.SecurityException)
                {
                }
                catch (BadImageFormatException)
                {
                }
                catch (FileLoadException)
                {
                }
            }

            return names;
        }
    }
}