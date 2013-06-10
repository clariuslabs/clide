#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace UnitTests
{
    using System;
    using System.Linq;
    using Autofac;
    using Clide.Composition;
    using Xunit;

    public class CompositionSpec
    {
        [Fact]
        public void when_registering_singleton_component_then_returns_same_instance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterComponents(typeof(RegularComponent), typeof(SingletonComponent));

            var container = builder.Build();

            var component1 = container.Resolve<SingletonComponent>();
            var component2 = container.Resolve<SingletonComponent>();

            Assert.Same(component1, component2);

            Assert.NotSame(container.Resolve<RegularComponent>(), container.Resolve<RegularComponent>());
        }

        [Component]
        public class RegularComponent
        {
        }

        [Component(IsSingleton = true)]
        public class SingletonComponent
        {
        }
    }
}