using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using System.ComponentModel.Composition.Hosting;
using Clide.Composition;

namespace Clide
{
	[TestClass]
	public class VsExportedServicesSpec : VsHostedSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[HostType("VS IDE")]
		[TestMethod]
		public void WhenGettingExportedServices_ThenSuccceedsForAll()
		{
			var getService = typeof(ExportProvider).GetMethod("GetExportedValue", new[] { typeof(string) });

			foreach (var export in typeof(VsExportedServices)
				.GetProperties()
				.Select(prop => new
				{
					ContractType = prop.PropertyType,
					ContractName = prop.GetCustomAttributes(typeof(ExportAttribute), false).OfType<ExportAttribute>().First().ContractName
				}))
			{
				var service = getService.MakeGenericMethod(export.ContractType).Invoke(Container, new object[] { export.ContractName });

				Assert.NotNull(service);
			}
		}
	}
}
