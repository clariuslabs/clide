using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;
using Moq;
using Xunit;

namespace Clide
{
    public class StartableServiceSpec
    {
        [Fact]
        public void when_starting_components_with_matching_case_insensitive_context_string_then_components_are_started()
        {
            var foo = new Mock<IStartable>();
            var bar = new Mock<IStartable>();

            var service = new StartableService(
                new[]
                {
                    new Lazy<IStartable, IStartableMetadata>
                    (
                        () => foo.Object,
                        Mock.Of<IStartableMetadata>(x => x.Context == "solutionLoaded")
                    ),
                    new Lazy<IStartable, IStartableMetadata>
                    (
                        () => bar.Object,
                        Mock.Of<IStartableMetadata>(x => x.Context == "SolutionLoaded")
                    )
                });

            service.StartComponentsAsync("solutionLoaded").Wait();

            foo.Verify(x => x.StartAsync());
            bar.Verify(x => x.StartAsync());
        }

        [Fact]
        public void when_starting_components_with_non_matching_context_string_then_components_are_not_started()
        {
            var foo = new Mock<IStartable>();

            var service = new StartableService(
                new[]
                {
                    new Lazy<IStartable, IStartableMetadata>
                    (
                        () => foo.Object,
                        Mock.Of<IStartableMetadata>(x => x.Context == "solutionLoaded")
                    )
                });

            service.StartComponentsAsync("packageLoaded").Wait();

            foo.Verify(x => x.StartAsync(), Times.Never);
        }

        [Fact]
        public void when_starting_components_with_context_guid_then_components_are_started()
        {
            var contextGuid = Guid.NewGuid();

            var foo = new Mock<IStartable>();
            var bar = new Mock<IStartable>();

            var service = new StartableService(
                new[]
                {
                        new Lazy<IStartable, IStartableMetadata>
                        (
                            () => foo.Object,
                            Mock.Of<IStartableMetadata>(x => x.ContextGuid == contextGuid)
                        ),
                        new Lazy<IStartable, IStartableMetadata>
                        (
                            () => bar.Object,
                            Mock.Of<IStartableMetadata>(x => x.ContextGuid == Guid.NewGuid())
                        )
                });

            service.StartComponentsAsync(contextGuid.ToString().ToLowerInvariant()).Wait();

            foo.Verify(x => x.StartAsync());
            bar.Verify(x => x.StartAsync(), Times.Never());
        }

        [Fact]
        public void when_first_component_cancels_task_then_second_component_is_not_started()
        {
            var cts = new CancellationTokenSource();

            var firtComponent = new CancellationComponent(cts);
            var secondComponent = new Mock<IStartable>();

            var service = new StartableService(
                new[]
                {
                    new Lazy<IStartable, IStartableMetadata>
                    (
                        () => firtComponent,
                        Mock.Of<IStartableMetadata>(x => x.Context == "solutionLoaded")
                    ),
                    new Lazy<IStartable, IStartableMetadata>
                    (
                        () => secondComponent.Object,
                        Mock.Of<IStartableMetadata>(x => x.Context == "solutionLoaded")
                    )
                });

            service.StartComponentsAsync("solutionLoaded", cts.Token).Wait();

            Assert.True(cts.IsCancellationRequested);
            secondComponent.Verify(x => x.StartAsync(), Times.Never);
        }

        class CancellationComponent : IStartable
        {
            readonly CancellationTokenSource cts;

            public CancellationComponent(CancellationTokenSource cts)
            {
                this.cts = cts;
            }

            public Task StartAsync()
            {
                cts.Cancel();

                return Task.CompletedTask;
            }
        }
    }
}
