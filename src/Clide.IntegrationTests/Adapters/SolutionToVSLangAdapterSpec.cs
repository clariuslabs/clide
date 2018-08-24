using System.Linq;
using VSLangProj;
using Xunit;

namespace Clide.Adapters
{
    [Collection("OpenSolution11")]
    public class SolutionToVsLangAdapterSpec
    {
        ISolutionFixture fixture;
        IAdapterService adapters;

        public SolutionToVsLangAdapterSpec(OpenSolution11Fixture fixture)
        {
            this.fixture = fixture;
            adapters = GlobalServiceLocator.Instance.GetExport<IAdapterService>();
        }

        [VsFact]
        public void when_adapting_project_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary");

            var to = adapters.Adapt(from).As<VSProject>();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_item_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary")
                .Nodes.OfType<IItemNode>().First(x => x.Name == "Class1.cs");

            var to = adapters.Adapt(from).As<VSProjectItem>();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_references_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary")
                .Nodes.OfType<IReferencesNode>().First();

            var to = adapters.Adapt(from).As<References>();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_reference_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary")
                .Nodes.OfType<IReferencesNode>()
                .SelectMany(x => x.Nodes.OfType<IReferenceNode>())
                .First();

            var to = adapters.Adapt(from).As<Reference>();

            Assert.NotNull(to);
        }

    }
}