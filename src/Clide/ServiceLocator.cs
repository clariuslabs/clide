using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Clide.Properties;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Clide
{
	internal class ServiceLocator : IServiceLocator
	{
		readonly IServiceProvider services;
		readonly ExportProvider exports;

		public ServiceLocator (IServiceProvider services)
			: this (services, services.GetService<SComponentModel, IComponentModel> ().DefaultExportProvider)
		{
		}

		public ServiceLocator (IServiceProvider services, ExportProvider exports)
		{
			this.services = services;
			this.exports = exports;
		}

		public object GetService (Type serviceType)
		{
			var service = services.GetService (serviceType);
			if (service == null)
				throw new MissingDependencyException (Strings.ServiceLocator.MissingDependency (serviceType));

			return service;
		}

		public object GetExport (Type contractType, string contractName = null)
		{
			if (contractName == null)
				contractName = AttributedModelServices.GetContractName (contractType);

			try {
				return exports.GetExportedValue<object> (contractName);
			} catch (ImportCardinalityMismatchException ex) {
				throw new MissingDependencyException(Strings.ServiceLocator.MissingDependency(contractName), ex);
			}
		}

		public Lazy<object, object> GetExport (Type contractType, Type metadataType, string contractName = null) => GetExports (contractType, metadataType, contractName).FirstOrDefault ();

		public IEnumerable<object> GetExports (Type contractType, string contractName = null)
		{
			if (contractName == null)
				contractName = AttributedModelServices.GetContractName (contractType);

			return exports.GetExportedValues<object> (contractName);
		}

		public IEnumerable<Lazy<object, object>> GetExports (Type contractType, Type metadataType, string contractName = null)
		{
			if (contractName == null)
				contractName = AttributedModelServices.GetContractName (contractType);

			return exports.GetExports (contractType, metadataType, contractName);
		}
	}
}
