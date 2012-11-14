using Microsoft.VisualStudio.Shell;

namespace Clide.Commands
{
    /// <summary>
    /// Adapts the <see cref="OleMenuCommand"/> class to the 
    /// <see cref="IMenuCommand"/> interface we expose.
    /// </summary>
    internal class OleMenuCommandAdapter : IMenuCommand
    {
        private OleMenuCommand oleCommand;

        public OleMenuCommandAdapter(OleMenuCommand oleCommand)
        {
            this.oleCommand = oleCommand;
        }

        public bool Enabled
        {
            get { return this.oleCommand.Enabled; }
            set { this.oleCommand.Enabled = value; }
        }

        public string Text
        {
            get { return this.oleCommand.Text; }
            set { this.oleCommand.Text = value; }
        }

        public bool Visible
        {
            get { return this.oleCommand.Visible; }
            set { this.oleCommand.Visible = value; }
        }

        public bool Checked
        {
            get { return this.oleCommand.Checked; }
            set { this.oleCommand.Checked = value; }
        }
    }
}
