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
    using System.ComponentModel.Composition;

    /// <summary>
    /// Exposes the Clide host for interaction by the hosting package.
    /// </summary>
    /// <typeparam name="THostingPackage">Type of the package hosting 
    /// Clide.</typeparam>
    public interface IHost<TPackage, TExport>
        where TPackage : Package, TExport
    {
        /// <summary>
        /// Ensures the package components are fully initialized.
        /// </summary>
        void Initialize(TPackage package);

        /// <summary>
        /// Composition service that can be used to satisfy imports on 
        /// components that haven't been created by MEF already. This 
        /// contains all exports provided by the hosting package as well 
        /// as Clide.
        /// </summary>
        ICompositionService Composition { get; }

        /// <summary>
        /// Clide will automatically load the hosting package whenever 
        /// a component imports the designated <typeparamref name="TExport"/> 
        /// export type. This ensures that no component is used before 
        /// the package has been fully initialized.
        /// </summary>
        Lazy<TExport> HostingPackage { get; }
    }
}
