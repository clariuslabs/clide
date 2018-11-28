using System;
using System.Collections.Generic;
using Moq;
using Clide.Commands;
using Xunit;
using System.ComponentModel.Design;

namespace Clide
{
    public class VsCommandExtensionAdapterSpec
    {
        static readonly CommandID TestCommandID = new CommandID(Guid.Empty, 1);

        [Fact]
        public void when_uicontext_is_active_then_query_status_is_called()
        {
            var command = new Mock<ICommandExtension>();

            var adapter = new VsCommandExtensionAdapter(TestCommandID, command.Object, new UIContextWrapper(true), true);

            command.Verify(x => x.QueryStatus(It.IsAny<IMenuCommand>()), Times.Once);
        }

        [Fact]
        public void when_uicontext_is_not_active_then_query_status_is_not_called()
        {
            var command = new Mock<ICommandExtension>();

            var adapter = new VsCommandExtensionAdapter(TestCommandID, command.Object, new UIContextWrapper(false), true);

            command.Verify(x => x.QueryStatus(It.IsAny<IMenuCommand>()), Times.Never);
        }

        [Fact]
        public void when_uicontext_is_not_active_then_adapter_is_disabled_and_invisible()
        {
            var command = new Mock<ICommandExtension>();

            var adapter = new VsCommandExtensionAdapter(TestCommandID, command.Object, new UIContextWrapper(false), true);

            Assert.False(adapter.Enabled);
            Assert.False(adapter.Visible);
        }

        [Fact]
        public void when_uicontext_is_active_then_command_is_executed()
        {
            var command = new Mock<ICommandExtension>();

            var adapter = new VsCommandExtensionAdapter(TestCommandID, command.Object, new UIContextWrapper(true));

            adapter.Invoke();

            command.Verify(x => x.Execute(It.IsAny<IMenuCommand>()), Times.Once);
        }

        [Fact]
        public void when_uicontext_is_not_active_then_command_is_not_executed()
        {
            var command = new Mock<ICommandExtension>();

            var adapter = new VsCommandExtensionAdapter(TestCommandID, command.Object, new UIContextWrapper(false));
            adapter.Invoke();

            command.Verify(x => x.Execute(It.IsAny<IMenuCommand>()), Times.Never);
        }

        [Fact]
        public void when_command_is_not_enabled_then_command_is_not_executed()
        {
            var command = new Mock<ICommandExtension>();
            // Disable the command by implementing the QueryStatus
            command
                .Setup(x => x.QueryStatus(It.IsAny<IMenuCommand>()))
                .Callback<IMenuCommand>(x =>
                {
                    x.Enabled = false;
                    x.Visible = false;
                });

            var adapter = new VsCommandExtensionAdapter(TestCommandID, command.Object);
            adapter.Invoke();

            // Ensure the QueryStatus is executed when the command is executed
            command.Verify(x => x.QueryStatus(It.IsAny<IMenuCommand>()), Times.AtLeastOnce());
            // Ensure the command is not executed based on the QueryStatus disable logic
            command.Verify(x => x.Execute(It.IsAny<IMenuCommand>()), Times.Never);
        }
    }
}
