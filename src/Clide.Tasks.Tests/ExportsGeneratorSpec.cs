using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using Xunit.Abstractions;

namespace Clide.Tasks.Tests
{
	public class ExportsGeneratorSpec
	{
		const string ComponentAttribute = "ComponentAttribute";

		ITestOutputHelper output;

		public ExportsGeneratorSpec(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public async Task when_processing_document_then_generates_exports()
		{
			var workspace = MSBuildWorkspace.Create();
			var project = workspace.CurrentSolution.AddProject("Clide", "Clide", "C#")
				.WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
				.AddMetadataReferences(Assembly.GetExecutingAssembly().GetReferencedAssemblies()
					.Select(name => MetadataReference.CreateFromFile(Assembly.Load(name).ManifestModule.FullyQualifiedName)))
				.AddMetadataReferences(Directory.EnumerateFiles(ModuleInitializer.BaseDirectory, "*.dll")
					.Select(file => MetadataReference.CreateFromFile(Assembly.LoadFrom(file).ManifestModule.FullyQualifiedName)));

			Debug.WriteLine(typeof(CreationPolicy));

			project = project
				.AddDocument("ComponentAttibute.cs", SourceText.From(
					File.ReadAllText(Path.Combine(ModuleInitializer.BaseDirectory, "Clide\\ComponentAttribute.cs"))))
				.Project;

			var document = project.AddDocument("Producer", CSharpSyntaxTree.ParseText(@"
using System;
using System.Reactive.Disposables;
using System.ComponentModel.Composition;

namespace Clide 
{
	[Component(CreationPolicy.Shared)]
	public partial class Producer : IObservable<DerivedEvent>, IDisposable, Microsoft.VisualStudio.Shell.Interop.IVsNonSolutionProjectFactory
	{
		public IDisposable Subscribe(IObserver<DerivedEvent> observer)
		{
			observer.OnNext(new DerivedEvent());
			return Disposable.Empty;
		}

		public void Dispose() { }
	}

	public class BaseEvent { }
	public class DerivedEvent : BaseEvent { }
}
").GetRoot());

			// The project is immutable, need to get the one from the document.
			project = document.Project;

			var compilation = await project.GetCompilationAsync();
			// There should be no errors/warnings at all.
			if (compilation.GetDiagnostics().Length > 0)
			{
				Assert.True(false, string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.ToString())));
			}

			var generator = new ExportsGenerator(compilation);
			var exports = generator.GenerateExports(new[] { document }, new HashSet<string>(new[] { "Microsoft.VisualStudio" })).ToArray();

			Assert.Equal(1, exports.Length);

			compilation = await exports[0].Project.GetCompilationAsync();
			// There should be no errors/warnings at all.
			if (compilation.GetDiagnostics().Length > 0)
			{
				Assert.True(false, string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.ToString())));
			}

			var text = await exports[0].GetTextAsync();

			output.WriteLine(text.ToString());
		}

		[Fact]
		public async Task when_action_then_assert()
		{
			var workspace = MSBuildWorkspace.Create();
			var projectPath = new FileInfo(@"..\..\..\Clide\Clide.csproj").FullName;

			await workspace.OpenSolutionAsync(@"..\..\..\Clide.sln");

			var project = workspace.CurrentSolution.Projects.Where(x =>
			   string.Equals(x.FilePath, projectPath, StringComparison.InvariantCultureIgnoreCase))
				.FirstOrDefault();

			var compilation = await project.GetCompilationAsync();

			Assert.NotNull(project);

			var observableAttribute = compilation
				.GetSymbolsWithName(name => name == ComponentAttribute, SymbolFilter.Type)
				.FirstOrDefault(a => a.ContainingNamespace.Name == "Clide");

			var observables = new List<INamedTypeSymbol>();

			foreach (var document in project.Documents)
			{
				var syntax = await document.GetSyntaxTreeAsync();
				var semanticModel = compilation.GetSemanticModel(syntax);
				var visitor = new ComponentVisitor(semanticModel, observableAttribute);
				visitor.Visit(syntax.GetRoot());

				observables.AddRange(visitor.observables);
			}

			foreach (var observable in observables)
			{
				var observableInterfaces = observable.AllInterfaces.Where(i =>
					i.IsGenericType && i.ConstructedFrom.ToString() == "System.IObservable<T>").ToList();

				Console.WriteLine(observableInterfaces.Count);
			}

			//var task = Mock.Of<Task>
			//var project = 
		}

		class ComponentVisitor : CSharpSyntaxWalker
		{
			public List<INamedTypeSymbol> observables = new List<INamedTypeSymbol>();
			ISymbol attribute;
			SemanticModel semanticModel;

			public ComponentVisitor(SemanticModel semanticModel, ISymbol attribute)
			{
				this.semanticModel = semanticModel;
				this.attribute = attribute;
			}

			public override void VisitClassDeclaration(ClassDeclarationSyntax node)
			{
				base.VisitClassDeclaration(node);

				var symbol = semanticModel.GetDeclaredSymbol(node);
				if (symbol != null && node.AttributeLists
						.SelectMany(list => list.Attributes)
						.Select(syntax => semanticModel.GetSymbolInfo(syntax))
						.Where(attr => attr.Symbol.ContainingType == attribute)
						.Any())
				{
					observables.Add(symbol);
				}
			}	
		}
	}
}
