using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel.Composition;
using Clide.Commands;
using Clide;

namespace IntegrationPackage
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	[Command(Constants.PackageGuid, Constants.CommandSet, Constants.cmdHelloClide)]
	public class SampleCommand : ICommandExtension
	{
        private IShellPackage package;

        public SampleCommand()
        {

        }

        [Import]
        public Lazy<IShellPackage> Package 
        {
            set { this.package = value.Value; } 
        }

        [Import]
        public IMessageBoxService Messages { get; set; }

		public string Text
		{
			get { return "Sample"; }
		}

		public void Execute(IMenuCommand command)
		{
            this.Messages.ShowInformation(string.Format(
                "Clide Version: {0}", typeof(IDevEnv).Assembly.GetName().Version));
        }

		public void QueryStatus(IMenuCommand command)
		{
			command.Enabled = command.Visible = true;
		}
	}
}
