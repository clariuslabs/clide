using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using Moq;
using Xunit;

namespace Clide
{
	public class ServiceLocatorExtensionsSpec
	{
		[Fact]
		public void when_getting_typed_export_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocator(Mock.Of<IServiceProvider>(), container);

			var foo = locator.GetExport<Foo>();

			Assert.NotNull (foo);
		}

		[Fact]
		public void when_getting_typed_export_with_metadata_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocator(Mock.Of<IServiceProvider>(), container);

			var foo = locator.GetExport<Foo, IFooMetadata>();

			Assert.NotNull (foo);
			Assert.Equal ("asdf", foo.Metadata.Id);
		}


		[Fact]
		public void when_getting_typed_export_with_metadata_dictionary_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocator(Mock.Of<IServiceProvider>(), container);

			var foo = locator.GetExport<Foo, IDictionary<string, object>>();

			Assert.NotNull (foo);
			Assert.Equal ("asdf", foo.Metadata["Id"]);
		}

		[Fact]
		public void when_getting_typed_exports_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocator(Mock.Of<IServiceProvider>(), container);

			var foo = locator.GetExports<Foo>().FirstOrDefault();

			Assert.NotNull (foo);
		}

		[Fact]
		public void when_getting_typed_exports_with_metadata_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocator(Mock.Of<IServiceProvider>(), container);

			var foo = locator.GetExports<Foo, IFooMetadata>().FirstOrDefault();

			Assert.NotNull (foo);
			Assert.Equal ("asdf", foo.Metadata.Id);
		}

		[Fact]
		public void when_getting_typed_exports_with_metadata_dictionary_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocator(Mock.Of<IServiceProvider>(), container);

			var foo = locator.GetExports<Foo, IDictionary<string, object>>().FirstOrDefault();

			Assert.NotNull (foo);
			Assert.Equal ("asdf", foo.Metadata["Id"]);
		}

		[Fact]
		public void when_retrieving_service_then_invokes_service_provider ()
		{
			var locator = new ServiceLocator(
				Mock.Of<IServiceProvider>(x => x.GetService(typeof(Foo)) == new Foo()),
				Mock.Of<ExportProvider>());

			var foo = locator.GetService(typeof(Foo));

			Assert.NotNull (foo);
		}

		[Fact]
		public void when_getting_locator_from_service_provider_then_retrieves_from_provider_component ()
		{
			var services = Mock.Of<IServiceProvider>(s => s.GetService(typeof(SComponentModel)) ==
				Mock.Of<IComponentModel>(c => c.GetService<IServiceLocatorProvider>() ==
					new Mock<IServiceLocatorProvider> { DefaultValue = DefaultValue.Mock }.Object));

			var locator = services.GetServiceLocator();

			Assert.NotNull (locator);
		}

		[Fact]
		public void when_getting_locator_from_null_service_provider_then_throws ()
		{
			Assert.Throws<ArgumentNullException> (() => ((IServiceProvider)null).GetServiceLocator ());
		}

		[Fact]
		public void when_getting_locator_from_null_dte_then_throws ()
		{
			Assert.Throws<ArgumentNullException> (() => ((DTE)null).GetServiceLocator ());
		}

		[Fact]
		public void when_getting_locator_from_null_project_then_throws ()
		{
			Assert.Throws<ArgumentNullException> (() => ((Project)null).GetServiceLocator ());
		}

		[Fact]
		public void when_getting_locator_from_null_vsproject_then_throws ()
		{
			Assert.Throws<ArgumentNullException> (() => ((IVsProject)null).GetServiceLocator ());
		}

		[Fact]
		public void when_getting_locator_from_null_vshierarchy_then_throws ()
		{
			Assert.Throws<ArgumentNullException> (() => ((IVsHierarchy)null).GetServiceLocator ());
		}
	}
}
