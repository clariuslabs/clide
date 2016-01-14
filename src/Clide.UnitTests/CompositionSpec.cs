using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
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
				new AssemblyCatalog (typeof (ServiceLocator).Assembly),
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
				return typeof (ServiceLocator).Assembly
					.GetTypes ()
					.Select (x => new { Type = x, Export = x.GetCustomAttributes (typeof (ExportAttribute), true).OfType<ExportAttribute> ().FirstOrDefault () })
					.Where (x => x.Export != null)
					.Select (x => new object[] { x.Export.ContractType ?? x.Type, x.Export.ContractName })
					.ToArray ();
			}
		}
	}

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
