using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Clide.Tasks
{
	internal class AttributedTypeCollector : CSharpSyntaxWalker
	{
		ISymbol attribute;
		SemanticModel semanticModel;

		public AttributedTypeCollector(SemanticModel semanticModel, ISymbol attribute)
		{
			this.semanticModel = semanticModel;
			this.attribute = attribute;
		}

		public IList<INamedTypeSymbol> AnnotatedTypes { get; } = new List<INamedTypeSymbol>();

		public override void VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			base.VisitClassDeclaration(node);

			var symbol = semanticModel.GetDeclaredSymbol(node);
			if (symbol != null && node.AttributeLists
					.SelectMany(list => list.Attributes)
					.Select(syntax => semanticModel.GetSymbolInfo(syntax))
					.Where(attr => attr.Symbol != null && attr.Symbol.ContainingType == attribute)
					.Any())
			{
				AnnotatedTypes.Add(symbol);
			}
		}
	}
}
