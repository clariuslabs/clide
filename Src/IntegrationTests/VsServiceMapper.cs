namespace Clide
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class VsServiceMapper : IDisposable
    {
        private static readonly Regex hasVersion = new Regex(@"^.*?(?=\d+)", RegexOptions.Compiled);
        private static readonly Regex ivsService = new Regex(@"(.*?\.)(IVs)(.*?)(\d*)$", RegexOptions.Compiled);

        private ConcurrentDictionary<string, string> nameMap = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, Type> typeMap = new ConcurrentDictionary<string, Type>();
        private BlockingCollection<Assembly> assembliesToProcess = new BlockingCollection<Assembly>();

        private CancellationTokenSource cancellation = new CancellationTokenSource();
        private bool isProcessing = true;

        public VsServiceMapper()
        {
            // First setup the event handler, just in case iterating 
            // the assemblies or types causes further assemblies to load, or 
            // if for whatever reason VS loads more assemblies while 
            // we're processing the loaded ones (which is an array, 
            // so we may miss assemblies otherwise).
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                isProcessing = true;
                assembliesToProcess.Add(assembly);
            }

            Task.Factory.StartNew(ProcessAssemblies);
        }

        public bool IsProcessing { get { return isProcessing; } }

        public Type TryMap(string typeName)
        {
            string serviceTypeName;
            if (!nameMap.TryGetValue(typeName, out serviceTypeName))
                // If there's no non-versioned type name, we just use the type name
                serviceTypeName = typeName;

            Type serviceType;
            if (!typeMap.TryGetValue(serviceTypeName, out serviceType))
                return null;

            return serviceType;
        }

        public void Dispose()
        {
            cancellation.Cancel();
            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoaded;
            assembliesToProcess.CompleteAdding();
        }

        private void ProcessAssemblies()
        {
            //Parallel.ForEach(
            //    assembliesToProcess.GetConsumingEnumerable(cancellation.Token),
            //    assembly =>
            foreach (var assembly in assembliesToProcess.GetConsumingEnumerable(cancellation.Token))
            {
                MapAssembly(assembly);

                // When we reach zero remaining assemblies, we do a pass to clean up 
                // mappings from IVs -> SVs
                if (assembliesToProcess.Count == 0)
                {
                    try
                    {
                        MapSVsServices();
                    }
                    finally
                    {
                        isProcessing = false;
                    }
                }
            };
        }

        private static bool HasGuidAttribute(Type type)
        {
            return type.IsDefined(typeof(GuidAttribute), true);
        }

        private void MapAssembly(Assembly assembly)
        {
            if (assembly.IsDynamic)
                return;

            foreach (var type in assembly.GetExportedTypes().Where(t => HasGuidAttribute(t)))
            {
                MapType(type);
            }
        }

        private void MapType(Type type)
        {
            typeMap.GetOrAdd(type.FullName, type);

            return;

            var version = hasVersion.Match(type.Name);
            if (!version.Success)
            {
                // Cleanup on MapSVsServices takes care of removing this map if 
                // there is a matching SVs* service to map to.
                typeMap.GetOrAdd(type.FullName, type);
                return;
            }

            var noVersionTypeName = type.Namespace + "." + version.Value;
            nameMap.GetOrAdd(type.FullName, noVersionTypeName);
            // We don't map the versioned type name in this case, since 
            // it won't resolve anyway via GetService.
        }

        private void MapSVsServices()
        {
            // This method attempts to find matching SVs services for their IVs counterparts.
            // In many cases, requesting the IVs version does not work and the SVs has to 
            // be retrieved instead. (besides being the VS recommendation)

            var orderedMap = typeMap.Select(pair => pair.Key).OrderBy(key => key).ToArray();

            var versionedNames = typeMap.Keys.ToArray()
                .Select(key => new { VersionedName = key, BaseName = hasVersion.Match(key) })
                .Where(pair => pair.BaseName.Success && typeMap.ContainsKey(pair.BaseName.Value))
                .ToList();

            foreach (var pair in versionedNames)
            {
                // In this case, we want to remove the type mapping of 
                // the versioned to the type, and replace it with a
                // name mapping from the versioned to the non-versioned 
                // type, but only if the non-versioned type exists in the 
                // type map.
                Type removed;
                typeMap.TryRemove(pair.VersionedName, out removed);
                nameMap.TryAdd(pair.VersionedName, pair.BaseName.Value);
            }

            var ivsNames = nameMap.Keys.ToArray()
                .Where(key => ivsService.IsMatch(key))
                .Select(key => new { IVsService = key, SVsService = ivsService.Replace(key, "$1SVs$3") })
                // Only those pairs where there is an existing type mapping for both IVs and SVs
                .Where(pair => typeMap.ContainsKey(pair.IVsService) && typeMap.ContainsKey(pair.SVsService))
                .ToList();

            foreach (var pair in ivsNames)
            {
                // In this case, we want to remove the type mapping of IVs
                // and add a name mapping from IVs to SVs
                Type removed;
                typeMap.TryRemove(pair.IVsService, out removed);
                nameMap.TryAdd(pair.IVsService, pair.SVsService);
            }
        }

        private void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            isProcessing = true;
            assembliesToProcess.Add(args.LoadedAssembly);
        }
    }

    public class ServiceMapper
    {
        public void Register(Assembly assembly)
        {
        }

        public void Initialize()
        {
        }


    }

    [TestClass]
    public class VsServiceMapperSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        private static readonly Regex hasVersion = new Regex(@"^.*(?=\d+)", RegexOptions.Compiled);
        private static readonly Regex ivsService = new Regex(@"(.*\.)(IVs)(.*?)(\d*)$", RegexOptions.Compiled);
        private const string svsReplacement = "$1SVs$3";

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenMapping_ThenCanRetrieveIVsShell4()
        {
            var serviceTypeName = typeof(IVsShell4).FullName;

            var serviceType = MapType(serviceTypeName);

            Assert.NotNull(serviceType);
            Assert.Equal(typeof(SVsShell), serviceType);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenMapping_ThenCanRetrieveDTE2()
        {
            var serviceTypeName = typeof(DTE2).FullName;

            var serviceType = MapType(serviceTypeName);

            Assert.NotNull(serviceType);
            Assert.Equal(typeof(DTE).FullName, serviceType.FullName);
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
                .SelectMany(asm => asm.GetExportedTypes())
                .FirstOrDefault(t => 
                    t.FullName == serviceTypeName ||
                    t.Name == serviceTypeName.Substring(serviceTypeName.LastIndexOf('.') + 1));
        }
    }
}