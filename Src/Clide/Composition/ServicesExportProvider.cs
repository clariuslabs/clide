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

			var serviceType = typeMap.GetOrAdd(serviceTypeName, typeName => MapType(typeName));

			if (serviceType == null)
			{
				//typeMap.TryRemove(serviceTypeName, out serviceType);
				return Enumerable.Empty<Export>();
			}

			var service = services.GetService(serviceType);
			if (service == null)
				return Enumerable.Empty<Export>();

			return new Export[] { new Export(serviceTypeName, () => service) };
		}

		private Type MapType(string serviceTypeName)
		{
			Type serviceType = FindInDomain(serviceTypeName);

			if (hasVersion.IsMatch(serviceTypeName))
			{
				var noVersionName = hasVersion.Match(serviceTypeName).Value;
				var noVersionType = FindInDomain(noVersionName);
				if (noVersionType != null)
					serviceType = noVersionType;
			}

			if (ivsService.IsMatch(serviceTypeName))
			{
				var sVsServiceName = ivsService.Replace(serviceTypeName, svsReplacement);
				var sVsServiceType = FindInDomain(sVsServiceName);
				if (sVsServiceType != null)
					serviceType = sVsServiceType;
			}

			return serviceType;
		}

		private Type FindInDomain(string serviceTypeName)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.Where(asm => !asm.IsDynamic)
				.SelectMany(asm => TryGetExportedTypes(asm))
				.FirstOrDefault(t =>
					t.FullName == serviceTypeName ||
					t.Name == serviceTypeName.Substring(serviceTypeName.LastIndexOf('.') + 1));
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
