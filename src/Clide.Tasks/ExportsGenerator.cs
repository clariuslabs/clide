using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Clide.Tasks
{
	public class ExportsGenerator
	{
		Compilation compilation;

		public ExportsGenerator(Compilation compilation)
		{
			this.compilation = compilation;
		}

		public IEnumerable<Document> GenerateExports(IEnumerable<Document> components, 
			ISet<string> excludedNamespaces,
			CancellationToken cancellation =	 default(CancellationToken))
		{
			return GenerateExportsAsync(components, excludedNamespaces, cancellation).Result;
		}

		async Task<IEnumerable<Document>> GenerateExportsAsync(IEnumerable<Document> components,
			ISet<string> excludedNamespaces, 
			CancellationToken cancellation)
		{
			var attribute = compilation.FindTypeByName("Clide", "Clide", "ComponentAttribute");
			var documents = new List<Document>();

			foreach (var document in components)
			{
				var syntax = await document.GetSyntaxTreeAsync(cancellation);
				if (cancellation.IsCancellationRequested)
					return Enumerable.Empty<Document>();

				var semanticModel = compilation.GetSemanticModel(syntax);
				var visitor = new AttributedTypeCollector(semanticModel, attribute);
				visitor.Visit(syntax.GetRoot());

				foreach (var component in visitor.AnnotatedTypes)
				{
					var text = new StringBuilder();
					text.AppendLine("using System.ComponentModel.Composition;").AppendLine();
					text.Append("namespace ").AppendLine(component.ContainingNamespace.ToString());
					text.AppendLine("{");

					var policy = component.GetAttributes()
						.First(a => a.AttributeClass.Name == "ComponentAttribute" && a.AttributeClass.ContainingNamespace.Name == "Clide")
						.ConstructorArguments.Select(arg => arg.Value).Cast<CreationPolicy>().FirstOrDefault();

					text.Append("\t")
						.Append("[PartCreationPolicy(CreationPolicy.")
						.Append(policy.ToString())
						.AppendLine(")]");

					var componentInterfaces = from iface in component.AllInterfaces
											  let fullName = iface.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
											  where fullName != "System.IDisposable" && 
												!excludedNamespaces.Any(ns => fullName.StartsWith(ns))
											  select iface;

					// Export each implemented interface
					foreach (var iface in componentInterfaces)
					{
						text.Append("\t")
							.Append("[Export(typeof(")
							.Append(iface.ToString())
							.AppendLine("))]");

						// Special-case IObservable<T>, since we need to export it with the contract 
						// of each of the base types of T in order to be able to retrieve them 
						// from the event stream by event base type.
						if (iface.IsGenericType && iface.ConstructedFrom.ToString() == "System.IObservable<T>")
						{
							var eventType = iface.TypeArguments[0].BaseType;
							while (eventType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != "object")
							{
								text.Append("\t")
									.Append("[Export(typeof(System.IObservable<")
									.Append(eventType.ToString())
									.AppendLine(">))]");

								eventType = eventType.BaseType;
							}
						}
					}

					text.Append("\t")
						.Append("partial class ")
						.Append(component.Name)
						.AppendLine(" { }")
						.AppendLine("}");

					documents.Add(document.Project.AddDocument(Path.ChangeExtension(document.Name, ".g.cs"),
						SourceText.From(text.ToString()), document.Folders));
				}
			}

			return documents;
		}
	}
}
