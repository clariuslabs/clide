using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel.Composition;
using Clide.Commands;

namespace IntegrationPackage
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	[Command(Constants.PackageGuid, Constants.CommandSet, Constants.CommandId)]
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

		public string Text
		{
			get { return "Sample"; }
		}

		public void Execute(IMenuCommand command)
		{
			MessageBox.Show("Hello World");
		}

		public void QueryStatus(IMenuCommand command)
		{
			command.Enabled = command.Visible = true;
		}
	}
}
