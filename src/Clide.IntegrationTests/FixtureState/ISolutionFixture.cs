using System;

namespace Clide
{
	public interface ISolutionFixture : IDisposable
	{
		ISolutionNode Solution { get; }
	}
}
