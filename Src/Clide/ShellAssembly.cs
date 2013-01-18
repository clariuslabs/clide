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
    using Clide.Properties;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Dynamic;

    /// <summary>
    /// Allows to work with the latest version of Microsoft.VisualStudio.Shell 
    /// that the current IDE supports.
    /// </summary>
    internal static class ShellAssembly
    {
        private static Regex shellName = new Regex(@"Microsoft.VisualStudio.Shell.(\d\d)\.\d");

        /// <summary>
        /// Gets the type from the latest version of VS Shell that's loaded in the app domain.
        /// </summary>
        public static dynamic GetType(string typeName)
        {
            var shellAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => shellName.IsMatch(asm.FullName))
                .Select(asm => new { Assembly = asm, Version = int.Parse(shellName.Match(asm.FullName).Groups[1].Value) })
                .OrderByDescending(asm => asm.Version)
                .Select(asm => asm.Assembly)
                .FirstOrDefault();

            if (shellAssembly == null)
                throw new InvalidOperationException(Strings.ShellAssembly.NotFound);

            return shellAssembly.GetType(typeName, true).AsDynamicReflection();
        }
    }
}
