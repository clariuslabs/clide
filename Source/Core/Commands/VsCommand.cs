namespace Clide.Commands
{
    using System;
    using System.ComponentModel.Design;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Base class for all VS Commands    
    /// </summary>
    public abstract class VsCommand : OleMenuCommand
    {
        private IServiceProvider serviceProvider;

        public VsCommand(IServiceProvider serviceProvider, EventHandler onExecute, CommandID id)
            : base(onExecute, id)
        {
            this.serviceProvider = serviceProvider;            
            this.BeforeQueryStatus += OnBeforeQueryStatus;            
        }

        protected IServiceProvider ServiceProvider
        {
            get
            {
                return serviceProvider;
            }
        }

        protected void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand command = sender as OleMenuCommand;

            command.Enabled = command.Visible = command.Supported = CanExecute(command);
        }

        protected virtual bool CanExecute(OleMenuCommand command)
        {
            return true;
        }
    }
}