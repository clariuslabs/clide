using Xunit;


namespace Clide
{
	[CollectionDefinition ("OpenSolution")]
	public class OpenSolutionCollection : ICollectionFixture<OpenSolutionFixture> { }

	public class OpenSolutionFixture : SolutionFixture
	{
		public OpenSolutionFixture () : base (Constants.LibrarySolution) { }
	}

	[CollectionDefinition ("OpenSolution11")]
	public class OpenSolution11Collection : ICollectionFixture<OpenSolution11Fixture> { }

	public class OpenSolution11Fixture : SolutionFixture
	{
		public OpenSolution11Fixture () : base (Constants.Library11Solution) { }
	}

	[CollectionDefinition ("SingleProject")]
	public class SingleProjectCollection : ICollectionFixture<SingleProjectFixture> { }

	public class SingleProjectFixture : SolutionFixture
	{
		public SingleProjectFixture () : base (Constants.SingleProjectSolution) { }
	}
}
