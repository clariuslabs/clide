using System;
using Moq;
using Xunit;

namespace Clide
{
	public class ServiceProviderExtensionsSpec
	{
		[Fact]
		public void WhenTryGetNonExistentService_ThenReturnsNull ()
		{
			var sp = new Mock<IServiceProvider>();

			var service = sp.Object.TryGetService<IFoo>();

			Assert.Null (service);
		}

		[Fact]
		public void WhenTryGetNonExistentServiceWithRegistration_ThenReturnsNull ()
		{
			var sp = new Mock<IServiceProvider>();

			var service = sp.Object.TryGetService<IFooReg, IFoo>();

			Assert.Null (service);
		}

		[Fact]
		public void WhenGettingNonExistingService_ThenThrowsInvalidOperationException ()
		{
			var sp = new Mock<IServiceProvider>();

			Assert.Throws<InvalidOperationException> (() => sp.Object.GetService<IFoo> ());
		}

		[Fact]
		public void WhenGettingNonExistingServiceWithRegistration_ThenThrowsInvalidOperationException ()
		{
			var sp = new Mock<IServiceProvider>();

			Assert.Throws<InvalidOperationException> (() => sp.Object.GetService<IFooReg, IFoo> ());
		}

		public interface IFoo : IFooReg { }
		public interface IFooReg { }
	}
}