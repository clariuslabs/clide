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
		public DecoratingReflectionCatalog(ComposablePartCatalog innerCatalog)
		{
            this.ImportDecorator = context => null;
            this.ExportDecorator = context => null;
			this.PartDecorator = context => { };
			this.innerCatalog = innerCatalog;
		}

		/// <summary>
		/// Gets or sets the decorator for a parts.
		/// </summary>
		public Action<DecoratingPartContext> PartDecorator { get; set; }

		/// <summary>
		/// Gets or sets the decorator for exports.
		/// </summary>
		public Func<DecoratingExportContext, ExportInfo> ExportDecorator { get; set; }

        /// <summary>
        /// Gets or sets the decorator for imports.
        /// </summary>
        public Func<DecoratingImportContext, ImportInfo> ImportDecorator { get; set; }

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
			return parts.Select(part => ReflectionModelServices.CreatePartDefinition(
				ReflectionModelServices.GetPartType(part),
				true,
				new Lazy<IEnumerable<ImportDefinition>>(() => part.ImportDefinitions.Select(import => VisitImport(part, import))),
				new Lazy<IEnumerable<ExportDefinition>>(() => part.ExportDefinitions.Select(export => VisitExport(part, export))),
				new Lazy<IDictionary<string, object>>(() => VisitPart(part)),
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
			var context = new DecoratingPartContext(def);
			PartDecorator(context);
			return context.NewMetadata;
		}

		private ExportDefinition VisitExport(ComposablePartDefinition part, ExportDefinition export)
		{
			var context = new DecoratingExportContext(part, export);
			var info = ExportDecorator(context);
            if (info == null)
                return export;

            return ReflectionModelServices.CreateExportDefinition(
                ReflectionModelServices.GetExportingMember(export),
                info.ContractName,
                new Lazy<IDictionary<string, object>>(() => info.Metadata ?? export.Metadata),
                this);
		}

        private ImportDefinition VisitImport(ComposablePartDefinition part, ImportDefinition import)
        {
            var contractImport = import as ContractBasedImportDefinition;
            if (contractImport == null)
                return import;

            var context = new DecoratingImportContext(part, import);
            var info = ImportDecorator(context);
            if (info == null)
                return import;

            if (ReflectionModelServices.IsImportingParameter(import))
            {
                return ReflectionModelServices.CreateImportDefinition(
                    ReflectionModelServices.GetImportingParameter(contractImport),
                    info.ContractName,
                    info.RequiredTypeIdentity ?? contractImport.RequiredTypeIdentity,
                    info.RequiredMetadata ?? contractImport.RequiredMetadata,
                    info.Cardinality.HasValue ? info.Cardinality.Value : contractImport.Cardinality,
                    info.RequiredCreationPolicy.HasValue ? info.RequiredCreationPolicy.Value : contractImport.RequiredCreationPolicy, 
                    this);
            }
            else
            {
                return ReflectionModelServices.CreateImportDefinition(
                    ReflectionModelServices.GetImportingMember(contractImport),
                    info.ContractName,
                    info.RequiredTypeIdentity ?? contractImport.RequiredTypeIdentity,
                    info.RequiredMetadata ?? contractImport.RequiredMetadata,
                    info.Cardinality.HasValue ? info.Cardinality.Value : contractImport.Cardinality,
                    info.IsRecomposable.HasValue ? info.IsRecomposable.Value : contractImport.IsRecomposable,
                    info.RequiredCreationPolicy.HasValue ? info.RequiredCreationPolicy.Value : contractImport.RequiredCreationPolicy,
                    this);
            }
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
