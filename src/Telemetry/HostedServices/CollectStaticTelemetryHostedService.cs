using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Byndyusoft.Telemetry.Providers.Interface;
using Microsoft.Extensions.Hosting;

namespace Byndyusoft.Telemetry.HostedServices
{
    public class CollectStaticTelemetryHostedService : IHostedService
    {
        private readonly IStaticTelemetryItemProvider[] _providers;

        public CollectStaticTelemetryHostedService(IEnumerable<IStaticTelemetryItemProvider> providers)
        {
            _providers = providers.ToArray();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StaticTelemetryItemsCollector.CollectDataFrom(_providers);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}