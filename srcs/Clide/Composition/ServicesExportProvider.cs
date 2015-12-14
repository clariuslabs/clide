namespace Clide.Composition
{
	using Clide.Diagnostics;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.ComponentModel.Composition.Hosting;
	using System.ComponentModel.Composition.Primitives;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;

	internal class ServicesExportProvider : ExportProvider
	{
		private static readonly ITracer tracer = Tracer.Get<ServicesExportProvider>();
		private static readonly Regex hasVersion = new Regex(@"^.*(?=\d+)", RegexOptions.Compiled);
		private static readonly Regex ivsService = new Regex(@"(.*\.)(IVs)(.*?)(\d*)$", RegexOptions.Compiled);
		private const string svsReplacement = "$1SVs$3";

		private ConcurrentDictionary<string, Type> typeMap = new ConcurrentDictionary<string, Type>();
		private ConcurrentDictionary<string, IEnumerable<Export>> serviceExports = new ConcurrentDictionary<string, IEnumerable<Export>> ();

		private IServiceProvider services;

		public ServicesExportProvider(IServiceProvider services)
		{
			this.services = services;
		}

		protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
		{
			var serviceTypeName = definition.ContractName;
			if (serviceTypeName.IndexOf('.') == -1 ||
				serviceTypeName.StartsWith("System.") ||
				serviceTypeName.StartsWith("Clide."))
				return Enumerable.Empty<Export>();

			return serviceExports.GetOrAdd (serviceTypeName, contractName => {
				var serviceType = typeMap.GetOrAdd(contractName, typeName => MapType(typeName));
				if (serviceType == null)
					return Enumerable.Empty<Export>();

				// NOTE: if we can retrieve a valid instance of the service at least once, we 
				// assume we'll be able to retrieve it later on. Note also that we don't return 
				// the single retrieved instance from the export, but rather provide a function 
				// that does the GetService call every time, since we're caching the export but 
				// we don't know if the service can be safely cached.
				var service = services.GetService(serviceType);
				if (service == null)
					return Enumerable.Empty<Export>();

				return new Export[] { new Export(serviceTypeName, () => services.GetService(serviceType)) };
			});
		}

		private Type MapType(string serviceTypeName)
		{
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.Where(asm => !asm.IsDynamic)
				.SelectMany(asm => TryGetExportedTypes(asm))
				.ToList();

			var serviceType = Find(serviceTypeName, types);

			if (hasVersion.IsMatch(serviceTypeName))
			{
				var noVersionName = hasVersion.Match(serviceTypeName).Value;
				var noVersionType = Find(noVersionName, types);
				if (noVersionType != null)
					serviceType = noVersionType;
			}

			if (ivsService.IsMatch(serviceTypeName))
			{
				var sVsServiceName = ivsService.Replace(serviceTypeName, svsReplacement);
				var sVsServiceType = Find(sVsServiceName, types);
				if (sVsServiceType != null)
					serviceType = sVsServiceType;
			}

			return serviceType;
		}

		private Type Find(string typeName, List<Type> typesInDomain)
		{
			return typesInDomain.FirstOrDefault(t =>
					t.FullName == typeName ||
					t.Name == typeName.Substring(typeName.LastIndexOf('.') + 1));
		}

		private static Type[] TryGetExportedTypes(System.Reflection.Assembly asm)
		{
			try
			{
				return asm.GetExportedTypes();
			}
			catch (Exception)
			{
				tracer.Warn("Failed to retrieve exported types from assembly {0}.", asm.FullName);
				return new Type[0];
			}
		}
	}
}
