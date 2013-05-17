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

namespace Clide.Commands
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using Clide.Diagnostics;
    using Clide.Properties;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Base Visual Studio adapter command that invokes an <see cref="ICommandExtension"/> implementation, 
    /// automatically shielding it for errors to avoid VS hangs, and starting an activity before 
    /// executing it.
    /// </summary>
    public class VsCommandExtensionAdapter : OleMenuCommand
    {
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsCommandExtensionAdapter"/> class with a 
        /// specific command identifier and implementation.
        /// </summary>
        public VsCommandExtensionAdapter(CommandID id, ICommandExtension implementation)
            : base(OnExecute, id)
        {
            Guard.NotNull(() => id, id);
            Guard.NotNull(() => implementation, implementation);

            this.tracer = Tracer.Get(this.GetType());

            this.Implementation = implementation;
            this.BeforeQueryStatus += this.OnBeforeQueryStatus;
        }

        protected ICommandExtension Implementation { get; private set; }

        private static void OnExecute(object sender, EventArgs e)
        {
            var command = (VsCommandExtensionAdapter)sender;
            var menu = new MenuCommand { Enabled = command.Enabled, Text = command.Text, Visible = command.Visible };

            using (command.tracer.StartActivity(Strings.VsCommandExtensionAdapter.ExecutingCommand(
                command.Text, command.Implementation.GetType().Name)))
            {
                command.tracer.ShieldUI(() =>
                {
                    command.Implementation.QueryStatus(menu);

                    if (menu.Enabled)
                    {
                        command.Implementation.Execute(menu);
                    }
                    else
                    {
                        command.tracer.Warn(Strings.VsCommandExtensionAdapter.CannotExecute(
                            menu.Text, command.Implementation.GetType().Name));
                    }
                }, Strings.VsCommandExtensionAdapter.ExecuteShieldMessage);
            }
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var command = (VsCommandExtensionAdapter)sender;
            // Preventively set to false.
            command.Enabled = false;
            var menu = new MenuCommand
            {
                Enabled = command.Enabled,
                Text = command.Implementation.Text,
                Visible = command.Visible
            };

            try
            {
                command.Implementation.QueryStatus(menu);
                command.Enabled = menu.Enabled;
                command.Visible = menu.Visible;
                command.Text = menu.Text;
                command.Checked = menu.Checked;
            }
            catch (Exception ex)
            {
                command.Enabled = command.Visible = false;
                command.tracer.Error(ex, Strings.VsCommandExtensionAdapter.QueryStatusShieldMessage);
            }
        }

        private class MenuCommand : IMenuCommand
        {
            public bool Enabled { get; set; }
            public string Text { get; set; }
            public bool Visible { get; set; }
            public bool Checked { get; set; }
        }
    }
}