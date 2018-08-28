using System;
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
                .Where(x =>
                    string.Equals(x.Metadata.Context, context, StringComparison.OrdinalIgnoreCase) ||
                    (contextGuid != Guid.Empty && x.Metadata.ContextGuid == contextGuid));

            foreach (var component in componentsToBeStarted.OrderBy(x => x.Metadata.Order))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                await component.Value.StartAsync();
            }
        }
    }
}
