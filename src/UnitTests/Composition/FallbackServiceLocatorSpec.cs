namespace UnitTests.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Xunit;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using Clide.Composition;

    public class FallbackServiceLocatorSpec
    {
        [Fact]
        public void when_primary_locator_throws_activationexception_then_secondary_service_is_queried()
        {
            var primary = new Mock<IServiceLocator>();
            var secondary = new Mock<IServiceLocator>();
            var foo = Mock.Of<IFoo>();

            primary.Setup(x => x.GetInstance(typeof(IFoo), null)).Throws<ActivationException>();
            secondary.Setup(x => x.GetInstance(typeof(IFoo), null)).Returns(foo);

            var fallback = new FallbackServiceLocator(primary.Object, secondary.Object);

            Assert.Same(foo, fallback.GetInstance(typeof(IFoo), null));
        }

        public interface IFoo { }
    }
}
