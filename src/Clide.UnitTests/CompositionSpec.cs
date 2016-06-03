using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reactive.Linq;
using Merq;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Moq;
using Xunit;

namespace Clide
{
	public class CompositionSpec
	{
		Mock<IServiceProvider> services;
		CompositionContainer container;

		public CompositionSpec ()
		{
			container = new CompositionContainer (new AggregateCatalog (
				new AssemblyCatalog (typeof (ServiceLocatorProvider).Assembly),
				new TypeCatalog (typeof (MockServiceProvider), typeof (MockHierarchyItemManager))));

			services = container.GetExportedValue<Mock<IServiceProvider>> ();
		}

		[MemberData ("GetExportedComponents", DisableDiscoveryEnumeration = true)]
		[Theory]
		public void when_retrieving_exported_component_then_succeeds (Type contractType, string contractName)
		{
			if (string.IsNullOrEmpty (contractName))
				contractName = AttributedModelServices.GetContractName (contractType);

			var export = container.GetExports(contractType, typeof(IDictionary<string, object>), contractName).FirstOrDefault();

			Assert.NotNull (export);

			var value = export.Value;

			Assert.NotNull (value);
		}

		public static IEnumerable<object[]> GetExportedComponents
		{
			get
			{
				return typeof (ServiceLocatorProvider).Assembly
					.GetTypes ()
					.Select (x => new { Type = x, Export = x.GetCustomAttributes (typeof (ExportAttribute), true).OfType<ExportAttribute> ().FirstOrDefault () })
					.Where (x => x.Export != null)
					.Select (x => new object[] { x.Export.ContractType ?? x.Type, x.Export.ContractName })
					.ToArray ();
			}
		}

		[Fact]
		public void when_composing_event_stream_then_can_subscribe_to_exported_observable ()
		{
			var container = new CompositionContainer (new AggregateCatalog (
				new AssemblyCatalog (typeof (ServiceLocatorProvider).Assembly),
				new TypeCatalog (typeof (MockObservable))));

			var stream = container.GetExportedValue<IEventStream> ();
			string value = null;

			stream.Of<string> ().Subscribe (s => value = s);

			Assert.Equal ("foo", value);
		}

		[Fact]
		public void when_composing_command_bus_then_can_execute_exported_command_handler ()
		{
			var container = new CompositionContainer (new AggregateCatalog (
				new AssemblyCatalog (typeof (ServiceLocatorProvider).Assembly),
				new TypeCatalog (typeof (MockCommandHandler))));

			var bus = container.GetExportedValue<ICommandBus> ();

			Assert.True (bus.CanHandle<MockCommand> ());
			Assert.True (bus.CanExecute (new MockCommand ()));
			bus.Execute (new MockCommand ());
		}
	}

	[Observable]
	public class MockObservable : IObservable<string>
	{
		public IDisposable Subscribe (IObserver<string> observer) =>
			new[] { "foo" }.ToObservable ().Subscribe (observer);
	}

	[CommandHandler]
	public class MockCommandHandler : ICommandHandler<MockCommand>
	{
		public bool CanExecute (MockCommand command) => true;

		public void Execute (MockCommand command)
		{
		}
	}

	public class MockCommand : ICommand { }
 
	[PartCreationPolicy (CreationPolicy.Shared)]
	public class MockServiceProvider
	{
		public MockServiceProvider ()
		{
			Instance = Mock.Of<IServiceProvider> (x =>
				x.GetService (typeof (SVsSolution)) ==
					new Mock<IVsSolution> ()
						.As<IVsHierarchy> ()
						.As<SVsSolution> ().Object &&
				x.GetService (typeof (SComponentModel)) ==
					Mock.Of<IComponentModel> (c =>
						 c.GetService<IVsHierarchyItemManager> () == Mock.Of<IVsHierarchyItemManager> ()) &&
				x.GetService (typeof (SVsShellMonitorSelection)) ==
					new Mock<SVsShellMonitorSelection> ()
						.As<IVsMonitorSelection> ().Object &&
				x.GetService (typeof (SVsUIShell)) ==
					new Mock<SVsUIShell> ()
						.As<IVsUIShell> ().Object
			);
		}

		[Export (typeof (SVsServiceProvider))]
		public IServiceProvider Instance { get; private set; }

		[Export (typeof (Mock<IServiceProvider>))]
		public Mock<IServiceProvider> InstanceMock { get { return Mock.Get (Instance); } }
	}

	[PartCreationPolicy (CreationPolicy.Shared)]
	public class MockHierarchyItemManager
	{

		public MockHierarchyItemManager ()
		{
			Mock = new Mock<IVsHierarchyItemManager> ();
		}

		[Export]
		public IVsHierarchyItemManager Instance { get { return Mock.Object; } }

		[Export (typeof (Mock<IVsHierarchyItemManager>))]
		public Mock<IVsHierarchyItemManager> Mock { get; private set; }
	}
}
