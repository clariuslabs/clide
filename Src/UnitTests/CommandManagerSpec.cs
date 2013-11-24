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

namespace UnitTests
{
    using Clide.Commands;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell.Interop;
    using Moq;
    using System;
    using System.ComponentModel.Design;
    using System.Linq;
    using Xunit;

    public class CommandManagerSpec
    {
        [Fact]
        public void when_added_command_exists_then_throws()
        {
            var manager = new CommandManager(
                Mock.Of<IServiceProvider>(x =>
                    x.GetService(typeof(SVsShell)) == Mock.Of<IVsShell>() &&
                    x.GetService(typeof(DTE)) == Mock.Of<DTE>(dte =>
                        dte.Events.get_CommandEvents(It.IsAny<string>(), It.IsAny<int>()) == Mock.Of<CommandEvents>()) &&
                    x.GetService(typeof(IMenuCommandService)) == Mock.Of<IMenuCommandService>(mcs =>
                        mcs.FindCommand(It.IsAny<CommandID>()) == new MenuCommand(null, new CommandID(Guid.Empty, 0)))),
                Enumerable.Empty<Lazy<ICommandExtension, CommandAttribute>>(),
                Enumerable.Empty<Lazy<ICommandFilter, CommandFilterAttribute>>(),
                Enumerable.Empty<Lazy<ICommandInterceptor, CommandInterceptorAttribute>>());

            Assert.Throws<ArgumentException>(() =>
                manager.AddCommand(Mock.Of<ICommandExtension>(), new CommandAttribute(Guid.NewGuid().ToString(), 5)));
        }
    }
}