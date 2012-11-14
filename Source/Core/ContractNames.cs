using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide
{
	/// <summary>
	/// Exposes the contract names for exports of general purpose services.
	/// </summary>
	public static class ContractNames
	{
		private const string Prefix = "Clide.";

		/// <summary>
		/// Contract name for accessing ICompositionService.
		/// </summary>
		public const string ICompositionService = Prefix + "ICompositionService";

		/// <summary>
		/// Contract name for accessing the global ExportProvider..
		/// </summary>
		public const string ExportProvider = Prefix + "ExportProvider";

		/// <summary>
		/// Contract name for accessing the Autofac container.
		/// </summary>
		public const string AutofacContainer = Prefix + "Autofac.IContainer";

		/// <summary>
		/// Contract name for exposing an Autofac module.
		/// </summary>
		public const string AutofacModule = Prefix + "Autofac.IModule";
	}
}
