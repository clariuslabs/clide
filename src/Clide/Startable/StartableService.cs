using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Clide
{
    [Export(typeof(IStartableService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class StartableService : IStartableService
    {
        readonly IEnumerable<Lazy<IStartable, IStartableMetadata>> components;
        readonly ConcurrentDictionary<IStartableMetadata, StartableContextParseResult> contextByMetadata =
            new ConcurrentDictionary<IStartableMetadata, StartableContextParseResult>();


        [ImportingConstructor]
        public StartableService([ImportMany] IEnumerable<Lazy<IStartable, IStartableMetadata>> components)
        {
            this.components = components;
        }

        public Task StartComponentsAsync(string context) =>
            StartComponentsAsync(context, CancellationToken.None);

        public async Task StartComponentsAsync(string context, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(context, out var contextGuid))
                contextGuid = Guid.Empty;

            var componentsToBeStarted = components
                .Where(component =>
                {
                    // Evaluate and cache the result of parsing the startable context
                    var metadataContext = contextByMetadata.GetOrAdd(
                        component.Metadata,
                        metadata => new StartableContextParseResult(metadata.Context));

                    // Use context guids
                    if (contextGuid != Guid.Empty)
                        return metadataContext.Guids.Any(x => x == contextGuid);

                    // Use context strings
                    return metadataContext.Values.Any(x => string.Equals(x, context, StringComparison.OrdinalIgnoreCase));
                });

            foreach (var component in componentsToBeStarted.OrderBy(x => x.Metadata.Order))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                await component.Value.StartAsync();
            }
        }

        class StartableContextParseResult
        {
            public StartableContextParseResult(string context)
            {
                Values = context.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();

                var guids = new List<Guid>();
                foreach (var value in Values)
                    if (Guid.TryParse(value, out var guid))
                        guids.Add(guid);

                Guids = guids;
            }

            public IEnumerable<string> Values { get; }

            public IEnumerable<Guid> Guids { get; }
        }
    }
}