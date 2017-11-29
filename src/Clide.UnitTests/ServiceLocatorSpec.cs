using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Microsoft.VisualStudio.ComponentModelHost;
using Moq;
using Xunit;

namespace Clide
{
	public class ServiceLocatorSpec
	{
		[Fact]
		public void when_getting_export_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocatorImpl(Mock.Of<IServiceProvider>(), new Lazy<ExportProvider>(() => container));

			var foo = locator.GetExport(typeof(Foo));

			Assert.NotNull (foo);
		}

		[Fact]
		public void when_getting_export_with_metadata_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocatorImpl(Mock.Of<IServiceProvider>(), new Lazy<ExportProvider>(() => container));

			var foo = locator.GetExport(typeof(Foo), typeof(IFooMetadata));

			Assert.NotNull (foo);
			Assert.Equal ("asdf", ((IFooMetadata)foo.Metadata).Id);
		}


		[Fact]
		public void when_getting_export_with_metadata_dictionary_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocatorImpl(Mock.Of<IServiceProvider>(), new Lazy<ExportProvider>(() => container));

			var foo = locator.GetExport(typeof(Foo), typeof(IDictionary<string, object>));

			Assert.NotNull (foo);
			Assert.Equal ("asdf", ((IDictionary<string, object>)foo.Metadata)["Id"]);
		}

		[Fact]
		public void when_getting_exports_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocatorImpl(Mock.Of<IServiceProvider>(), new Lazy<ExportProvider>(() => container));

			var foo = locator.GetExports(typeof(Foo)).FirstOrDefault();

			Assert.NotNull (foo);
		}

		[Fact]
		public void when_getting_exports_with_metadata_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocatorImpl(Mock.Of<IServiceProvider>(), new Lazy<ExportProvider>(() => container));

			var foo = locator.GetExports(typeof(Foo), typeof(IFooMetadata)).FirstOrDefault();

			Assert.NotNull (foo);
			Assert.Equal ("asdf", ((IFooMetadata)foo.Metadata).Id);
		}

		[Fact]
		public void when_getting_exports_with_metadata_dictionary_then_succeeds ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocatorImpl(Mock.Of<IServiceProvider>(), new Lazy<ExportProvider>(() => container));

			var foo = locator.GetExports(typeof(Foo), typeof(IDictionary<string, object>)).FirstOrDefault();

			Assert.NotNull (foo);
			Assert.Equal ("asdf", ((IDictionary<string, object>)foo.Metadata)["Id"]);
		}

		[Fact]
		public void when_retrieving_service_then_invokes_service_provider ()
		{
			var locator = new ServiceLocatorImpl(
				Mock.Of<IServiceProvider>(x => x.GetService(typeof(Foo)) == new Foo()),
				new Lazy<ExportProvider>(() => Mock.Of<ExportProvider>()));

			var foo = locator.GetService(typeof(Foo));

			Assert.NotNull (foo);
		}

		[Fact]
		public void when_constructing_locator_with_service_provider_then_gets_exports_from_component_model_service ()
		{
			var container = new CompositionContainer(new TypeCatalog(typeof(Foo)));
			var locator = new ServiceLocatorImpl(
				Mock.Of<IServiceProvider>(s => s.GetService(typeof(SComponentModel)) ==
					Mock.Of<IComponentModel>(c => c.DefaultExportProvider == container)));

			var foo = locator.GetExport<Foo>();

			Assert.NotNull (foo);
		}

		[Fact]
		public void when_getting_missing_export_then_throws ()
		{
			var container = new CompositionContainer();
			var locator = new ServiceLocatorImpl(Mock.Of<IServiceProvider>(), new Lazy<ExportProvider>(() => container));

			Assert.Throws<MissingDependencyException> (() => locator.GetExport (typeof (Foo)));
		}

		[Fact]
		public void when_getting_missing_exports_then_returns_empty ()
		{
			var container = new CompositionContainer();
			var locator = new ServiceLocatorImpl(Mock.Of<IServiceProvider>(), new Lazy<ExportProvider>(() => container));

			var instances = locator.GetExports (typeof (Foo)).ToList ();

			Assert.Empty(instances);
		}

		[Fact]
		public void when_getting_missing_service_then_throws ()
		{
			var container = new CompositionContainer();
			var locator = new ServiceLocatorImpl(Mock.Of<IServiceProvider>(), new Lazy<ExportProvider>(() => container));

			Assert.Throws<MissingDependencyException> (() => locator.GetService (typeof (Foo)));
		}
	}

	[FooExport ("asdf")]
	public class Foo { }

	public interface IFooMetadata
	{
		string Id { get; }
	}

	[MetadataAttribute]
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class FooExportAttribute : ExportAttribute, IFooMetadata
	{
		public FooExportAttribute (string id)
		{
			this.Id = id;
		}

		public string Id { get; set; }
	}
}
