using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Clide
{
    public class StartableServiceSpec
    {
        [Fact]
        public async Task when_starting_components_with_matching_case_insensitive_context_string_then_components_are_started()
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

            await service.StartComponentsAsync("solutionLoaded");

            foo.Verify(x => x.StartAsync());
            bar.Verify(x => x.StartAsync());
        }

        [Fact]
        public async Task when_starting_components_with_non_matching_context_string_then_components_are_not_started()
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

            await service.StartComponentsAsync("packageLoaded");

            foo.Verify(x => x.StartAsync(), Times.Never);
        }

        [Fact]
        public async Task when_starting_components_with_context_guid_then_components_are_started()
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

            await service.StartComponentsAsync(contextGuid.ToString().ToLowerInvariant());

            foo.Verify(x => x.StartAsync());
            bar.Verify(x => x.StartAsync(), Times.Never());
        }

        [Fact]
        public async Task when_first_component_cancels_task_then_second_component_is_not_started()
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

            await service.StartComponentsAsync("solutionLoaded", cts.Token);

            Assert.True(cts.IsCancellationRequested);
            secondComponent.Verify(x => x.StartAsync(), Times.Never);
        }

        [Fact]
        public async Task when_starting_components_with_order_then_components_are_started_honoring_the_provided_order()
        {
            var components = new List<string>();

            var foo = new Mock<IStartable>();
            foo.Setup(x => x.StartAsync()).Callback(() => components.Add("foo")).Returns(Task.CompletedTask);

            var bar = new Mock<IStartable>();
            bar.Setup(x => x.StartAsync()).Callback(() => components.Add("bar")).Returns(Task.CompletedTask);

            var service = new StartableService(
                new[]
                {
                        new Lazy<IStartable, IStartableMetadata>
                        (
                            () => foo.Object,
                            Mock.Of<IStartableMetadata>(x => x.Context == "order" && x.Order == 100)
                        ),
                        new Lazy<IStartable, IStartableMetadata>
                        (
                            () => bar.Object,
                            Mock.Of<IStartableMetadata>(x => x.Context == "order" && x.Order == 1)
                        )
                });

            await service.StartComponentsAsync("order");

            // Bar should be started before Foo
            Assert.Equal(2, components.Count);
            Assert.Equal("bar", components[0]);
            Assert.Equal("foo", components[1]);
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
