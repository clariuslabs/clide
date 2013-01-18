#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Composition
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.ComponentModel.Composition.ReflectionModel;
    using System.Linq;

    /// <summary>
	/// Custom catalog that allows decorating a reflection-based catalog with 
	/// custom export and part metadata.
	/// </summary>
	internal class DecoratingReflectionCatalog : ComposablePartCatalog, ICompositionElement
	{
		private readonly ComposablePartCatalog innerCatalog;
		private IEnumerable<ComposablePartDefinition> cachedSharedParts;

		/// <summary>
		/// Initializes the catalog.
		/// </summary>
		/// <param name="innerCatalog"></param>
		public DecoratingReflectionCatalog(ComposablePartCatalog innerCatalog)
		{
			this.ExportMetadataDecorator = context => { };
			this.PartMetadataDecorator = context => { };
			this.innerCatalog = innerCatalog;
			// TODO: detect changes in inner catalog.
		}

		/// <summary>
		/// Gets or sets the decorator for a parts metadata.
		/// </summary>
		public Action<DecoratedPart> PartMetadataDecorator { get; set; }

		/// <summary>
		/// Gets or sets the decorator for exports metadata.
		/// </summary>
		public Action<DecoratedExport> ExportMetadataDecorator { get; set; }

		/// <summary>
		/// Applies the decorations and gets the parts definitions.
		/// </summary>
		public override IQueryable<ComposablePartDefinition> Parts
		{
			get
			{
				if (this.cachedSharedParts == null)
				{
					this.cachedSharedParts = this.BuildSharedParts().ToList();
				}

				return this.cachedSharedParts.Concat(BuildNonSharedParts())
					.Distinct(new SelectorEqualityComparer<ComposablePartDefinition, Type>(def => ReflectionModelServices.GetPartType(def).Value))
					.AsQueryable();
			}
		}

		private IEnumerable<ComposablePartDefinition> BuildSharedParts()
		{
			return BuildParts(innerCatalog.Parts.Where(def => !IsNonShared(def)));
		}

		private IEnumerable<ComposablePartDefinition> BuildNonSharedParts()
		{
			return BuildParts(innerCatalog.Parts.Where(def => IsNonShared(def)));
		}

		private IEnumerable<ComposablePartDefinition> BuildParts(IQueryable<ComposablePartDefinition> parts)
		{
			return parts.Select(def => ReflectionModelServices.CreatePartDefinition(
				ReflectionModelServices.GetPartType(def),
				true,
				new Lazy<IEnumerable<ImportDefinition>>(() => def.ImportDefinitions),
				new Lazy<IEnumerable<ExportDefinition>>(() => def.ExportDefinitions.Select(export =>
					ReflectionModelServices.CreateExportDefinition(
						ReflectionModelServices.GetExportingMember(export),
						export.ContractName,
						new Lazy<IDictionary<string, object>>(() => VisitExport(def, export)),
						this))),
				new Lazy<IDictionary<string, object>>(() => VisitPart(def)),
				this));
		}

		private bool IsNonShared(ComposablePartDefinition def)
		{
			var metadata = VisitPart(def);
			return metadata.ContainsKey(CompositionConstants.PartCreationPolicyMetadataName) &&
				(CreationPolicy)metadata[CompositionConstants.PartCreationPolicyMetadataName] == CreationPolicy.NonShared;
		}

		private IDictionary<string, object> VisitPart(ComposablePartDefinition def)
		{
			var context = new DecoratedPart(def);
			PartMetadataDecorator(context);
			return context.NewMetadata;
		}

		private IDictionary<string, object> VisitExport(ComposablePartDefinition part, ExportDefinition export)
		{
			var context = new DecoratedExport(part, export);
			ExportMetadataDecorator(context);
			return context.NewMetadata;
		}

		/// <summary>
		/// Disposes the inner catalog.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				innerCatalog.Dispose();
		}

		string ICompositionElement.DisplayName
		{
			get
			{
				var composition = innerCatalog as ICompositionElement;
				if (composition == null)
					return "Decorating Catalog";
				else
					return "Decorating Catalog for " + composition.DisplayName;
			}
		}

		ICompositionElement ICompositionElement.Origin
		{
			get { return innerCatalog as ICompositionElement; }
		}
	}
}
