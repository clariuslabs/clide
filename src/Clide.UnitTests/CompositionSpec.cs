using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reactive.Linq;
using Merq;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Moq;
using Xunit;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace Clide
{
    public class CompositionSpec
    {
        Mock<IServiceProvider> services;
        CompositionContainer container;

        public CompositionSpec()
        {
            container = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(ServiceLocatorProvider).Assembly),
                new AssemblyCatalog(typeof(MockServiceProvider).Assembly)));

            ExportProviderProvider.SetExportProvider(container);

            services = container.GetExportedValue<Mock<IServiceProvider>>();
        }

        [MemberData(nameof(GetExportedComponents), DisableDiscoveryEnumeration = true)]
        [Theory]
        public void when_retrieving_exported_component_then_succeeds(Type contractType, string contractName)
        {
            if (string.IsNullOrEmpty(contractName))
                contractName = AttributedModelServices.GetContractName(contractType);

            var export = container.GetExports(contractType, typeof(IDictionary<string, object>), contractName).FirstOrDefault();

            Assert.NotNull(export);

            var value = export.Value;

            Assert.NotNull(value);
        }

        public static IEnumerable<object[]> GetExportedComponents
        {
            get
            {
                return typeof(ServiceLocatorProvider).Assembly
                    .GetTypes()
                    .Select(x => new { Type = x, Export = x.GetCustomAttributes(typeof(ExportAttribute), true).OfType<ExportAttribute>().FirstOrDefault() })
                    .Where(x => x.Export != null)
                    .Select(x => new object[] { x.Export.ContractType ?? x.Type, x.Export.ContractName })
                    .ToArray();
            }
        }

        [Fact]
        public void when_retrieving_services_shared_part_then_shares_instance()
        {
            var container = new CompositionContainer(new TypeCatalog(typeof(MockComponent)));

            var service = container.GetExportedValue<IService>();
            var component = container.GetExportedValue<IComponent>();

            Assert.Same(service, component);
        }
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TestJoinableTaskContext
    {
        public TestJoinableTaskContext()
        {
#pragma warning disable VSSDK005 // Avoid instantiating JoinableTaskContext
            Instance = new JoinableTaskContext();
#pragma warning restore VSSDK005 // Avoid instantiating JoinableTaskContext
        }

        [Export]
        public JoinableTaskContext Instance { get; }
    }

    [Export(typeof(IObservable<string>))]
    public class MockObservable : IObservable<string>
    {
        public IDisposable Subscribe(IObserver<string> observer) =>
            new[] { "foo" }.ToObservable().Subscribe(observer);
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MockServiceProvider
    {
        [ImportingConstructor]
        public MockServiceProvider(ExportProvider exports)
        {
            object zombie = false;

            var vsShell = new Mock<SVsShell>()
                .As<IVsShell>();
            vsShell.Setup(x => x.GetProperty((int)__VSSPROPID.VSSPROPID_Zombie, out zombie)).Returns(0);

            Instance = Mock.Of<IServiceProvider>(x =>
               x.GetService(typeof(SVsSolution)) ==
                   new Mock<IVsSolution>()
                       .As<IVsHierarchy>()
                       .As<SVsSolution>().Object &&
               x.GetService(typeof(SComponentModel)) ==
                   Mock.Of<IComponentModel>(c =>
                       c.GetService<IVsHierarchyItemManager>() == Mock.Of<IVsHierarchyItemManager>() &&
                       c.DefaultExportProvider == exports) &&
               x.GetService(typeof(SVsShellMonitorSelection)) ==
                   new Mock<SVsShellMonitorSelection>()
                       .As<IVsMonitorSelection>().Object &&
               x.GetService(typeof(SVsShell)) == vsShell.Object &&
               x.GetService(typeof(SVsFeatureFlags)) == Mock.Of<IVsFeatureFlags>() && 
               x.GetService(typeof(SVsUIShell)) ==
                   new Mock<SVsUIShell>()
                       .As<IVsUIShell>().Object
            );
        }

        [Export(typeof(SVsServiceProvider))]
        public IServiceProvider Instance { get; }

        [Export(typeof(Mock<IServiceProvider>))]
        public Mock<IServiceProvider> InstanceMock { get { return Mock.Get(Instance); } }
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MockAsyncServiceProvider
    {
        [ImportingConstructor]
        public MockAsyncServiceProvider(ExportProvider exports)
        {
            var mock = new Mock<IAsyncServiceProvider>();

            mock
                .Setup(x => x.GetServiceAsync(typeof(SComponentModel)))
                .ReturnsAsync(Mock.Of<IComponentModel>(c =>
                    c.GetService<IVsHierarchyItemManager>() == Mock.Of<IVsHierarchyItemManager>() &&
                    c.DefaultExportProvider == exports));

            Instance = mock.Object;
        }

        [Export(typeof(SAsyncServiceProvider))]
        public IAsyncServiceProvider Instance { get; }
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MockHierarchyItemManager
    {
        public MockHierarchyItemManager()
        {
            Mock = new Mock<IVsHierarchyItemManager>();
        }

        [Export]
        public IVsHierarchyItemManager Instance { get { return Mock.Object; } }

        [Export(typeof(Mock<IVsHierarchyItemManager>))]
        public Mock<IVsHierarchyItemManager> Mock { get; private set; }
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IComponent))]
    [Export(typeof(IService))]
    public class MockComponent : IComponent, IService
    {
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ExportProviderProvider
    {
        static ExportProvider exports;

        public static void SetExportProvider(ExportProvider exports)
        {
            ExportProviderProvider.exports = exports;
        }

        [Export]
        public ExportProvider Exports { get { return exports; } }
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CpsProvider
    {
        [Export]
        public UnconfiguredProject UnconfiguredProject { get; } = Mock.Of<UnconfiguredProject>();
    }

    public class MockCommandBusProvider
    {
        [Export(typeof(ICommandBus))]
        public ICommandBus Value => Mock.Of<ICommandBus>();
    }

    public class MockEventStreamProvider
    {
        [Export(typeof(IEventStream))]
        public IEventStream Value => Mock.Of<IEventStream>();
    }

    public interface IComponent { }
    public interface IService { }
}
