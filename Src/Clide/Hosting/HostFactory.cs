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

namespace Clide
{
    using Microsoft.VisualStudio.Shell;
    using System;

    /// <summary>
    /// Initializes the clide host.
    /// </summary>
    public static class HostFactory
    {
        /// <summary>
        /// Creates the host for the components.
        /// </summary>
        /// <typeparam name="TPackage">The type of the hosting package.</typeparam>
        /// <typeparam name="TExport">The type of exported contract for the hosting package.</typeparam>
        /// <param name="globalServiceProvider">The global service provider for Visual Studio, 
        /// from <see cref="ServiceProvider.GlobalServiceProvider"/>.</param>
        /// <param name="catalogName">Name of the catalog for the components, which must be the one used 
        /// as the [VsCatalogName("...")] in the hosting package AssemblyInfo file.</param>
        /// <returns></returns>
        public static IHost<TPackage, TExport> CreateHost<TPackage, TExport>(IServiceProvider globalServiceProvider, string catalogName)
            where TPackage : Package, TExport
        {
            return new Host<TPackage, TExport>(globalServiceProvider, catalogName);
        }
    }
}