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
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Shell;

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

        [Fact]
        public void when_adding_commands_then_skips_commands_for_other_packages()
        {
			var menusvc = new Mock<IMenuCommandService> ();

			var package = new Mock<MockPackage> ().As<IServiceProvider>();
			package.Setup(x => x.GetService(typeof(IMenuCommandService))).Returns(menusvc.Object);
			package.Setup(x => x.GetService(typeof(SVsShell))).Returns(Mock.Of<IVsShell>());
			package.Setup(x => x.GetService(typeof(DTE))).Returns(Mock.Of<DTE>(dte =>
                        dte.Events.get_CommandEvents(It.IsAny<string>(), It.IsAny<int>()) == Mock.Of<CommandEvents>()));

            var manager = new CommandManager(package.Object,
				new Lazy<ICommandExtension, CommandAttribute>[] {
					new Lazy<ICommandExtension, CommandAttribute>(() => Mock.Of<ICommandExtension>(), new CommandAttribute("CCB471F4-B764-4B1C-BEB7-26AFE5B9F48E", Guid.NewGuid().ToString(), 1)),
					new Lazy<ICommandExtension, CommandAttribute>(() => Mock.Of<ICommandExtension>(), new CommandAttribute("CD7DB44E-0472-49C8-92C5-046677F55E27", Guid.NewGuid().ToString(), 1)),
				},
                Enumerable.Empty<Lazy<ICommandFilter, CommandFilterAttribute>>(),
                Enumerable.Empty<Lazy<ICommandInterceptor, CommandInterceptorAttribute>>());

			manager.AddCommands ();

			menusvc.Verify (x => x.AddCommand (It.IsAny<MenuCommand> ()), Times.Once ());
        }

        [Fact]
        public void when_adding_commands_then_registers_commands_with_empty_package_guid()
        {
			var menusvc = new Mock<IMenuCommandService> ();

			var package = new Mock<MockPackage> ().As<IServiceProvider>();
			package.Setup(x => x.GetService(typeof(IMenuCommandService))).Returns(menusvc.Object);
			package.Setup(x => x.GetService(typeof(SVsShell))).Returns(Mock.Of<IVsShell>());
			package.Setup(x => x.GetService(typeof(DTE))).Returns(Mock.Of<DTE>(dte =>
                        dte.Events.get_CommandEvents(It.IsAny<string>(), It.IsAny<int>()) == Mock.Of<CommandEvents>()));

            var manager = new CommandManager(package.Object,
				new Lazy<ICommandExtension, CommandAttribute>[] {
					new Lazy<ICommandExtension, CommandAttribute>(() => Mock.Of<ICommandExtension>(), new CommandAttribute("CCB471F4-B764-4B1C-BEB7-26AFE5B9F48E", Guid.NewGuid().ToString(), 1)),
					new Lazy<ICommandExtension, CommandAttribute>(() => Mock.Of<ICommandExtension>(), new CommandAttribute(Guid.Empty.ToString(), Guid.NewGuid().ToString(), 1)),
				},
                Enumerable.Empty<Lazy<ICommandFilter, CommandFilterAttribute>>(),
                Enumerable.Empty<Lazy<ICommandInterceptor, CommandInterceptorAttribute>>());

			manager.AddCommands ();

			menusvc.Verify (x => x.AddCommand (It.IsAny<MenuCommand> ()), Times.Exactly(2));
        }

		[Guid("CCB471F4-B764-4B1C-BEB7-26AFE5B9F48E")]
		public class MockPackage : Package
		{
		}
    }
}