using Clide;
using Clide.Commands;

namespace IntegrationPackage
{
	[Command(Constants.CommandSet, Constants.cmdHelloClide)]
	public class SampleCommand : ICommandExtension
	{
        private IDevEnv devEnv;

        public SampleCommand(IDevEnv devEnv)
        {
            this.devEnv = devEnv;
        }

		public string Text
		{
			get { return "Sample"; }
		}

		public void Execute(IMenuCommand command)
		{
            devEnv.MessageBoxService.ShowInformation(string.Format(
                    "Clide Version: {0}", typeof(IDevEnv).Assembly.GetName().Version));
        }

		public void QueryStatus(IMenuCommand command)
		{
			command.Enabled = command.Visible = true;
		}
	}
}
