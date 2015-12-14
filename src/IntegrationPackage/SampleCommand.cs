using Clide;
using Clide.Commands;
using System.Linq;

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
            var items = devEnv.SolutionExplorer()
                .Solution
                .Traverse()
                .Count();

            devEnv.MessageBoxService.ShowInformation(string.Format(
                    "Clide Version: {0}, Solution Nodes: {1}",
                    typeof(IDevEnv).Assembly.GetName().Version,
                    items));
        }

        public void QueryStatus(IMenuCommand command)
        {
            command.Enabled = command.Visible = true;
        }
    }
}
