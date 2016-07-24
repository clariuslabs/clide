using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;

namespace Clide.Tasks
{
	static class Extensions
	{
		public static INamedTypeSymbol FindTypeByName(this Compilation compilation, string assemblyName, string containingNamespace, string typeName)
		{
			return (INamedTypeSymbol)compilation
				// It can be available as source, such as in Clide.csproj itself
				.GetSymbolsWithName(name => name == typeName, SymbolFilter.Type)
				.FirstOrDefault(a => a.ContainingNamespace.Name == containingNamespace) ?? compilation
					// Or via an assembly reference
					.References.Where(r => r.Properties.Kind == MetadataImageKind.Assembly)
					.Select(r => compilation.GetAssemblyOrModuleSymbol(r))
					.OfType<IAssemblySymbol>()
					.Where(a => a.Name == assemblyName)
					.Select(a => a.GetTypeByMetadataName(containingNamespace + "." +  typeName))
					.FirstOrDefault();
		}

		public static IEnumerable<KeyValuePair<ITaskItem, Document>> FindDocuments(this Project project, ITaskItem[] items, CancellationToken cancellation)
		{
			foreach (var item in items)
			{
				if (cancellation.IsCancellationRequested)
					break;

				var document = project.Documents.FirstOrDefault(doc => 
					doc.FilePath.Equals(item.GetMetadata("FullPath"), StringComparison.OrdinalIgnoreCase));

				if (document != null)
					yield return new KeyValuePair<ITaskItem, Document>(item, document);
			}
		}

		public static bool HasAttributedType(this Document document, ISymbol attribute, CancellationToken cancellation)
		{
			if (cancellation.IsCancellationRequested)
				return false;

			var syntax = document.GetSyntaxTreeAsync(cancellation).Result;
			if (cancellation.IsCancellationRequested)
				return false;

			var compilation = document.Project.GetCompilationAsync(cancellation).Result;
			if (cancellation.IsCancellationRequested)
				return false;

			var semanticModel = compilation.GetSemanticModel(syntax);
			if (cancellation.IsCancellationRequested)
				return false;

			var visitor = new AttributedTypeCollector(semanticModel, attribute);
			visitor.Visit(syntax.GetRoot());

			return visitor.AnnotatedTypes.Any();
		}
	}
}