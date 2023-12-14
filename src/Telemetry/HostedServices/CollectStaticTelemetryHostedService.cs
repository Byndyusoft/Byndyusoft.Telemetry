using System.Threading;
using System.Threading.Tasks;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface;
using Microsoft.Extensions.Hosting;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.HostedServices
{
    public class CollectStaticTelemetryHostedService : IHostedService
    {
        private readonly IStaticTelemetryItemProvider[] _providers;

        public CollectStaticTelemetryHostedService(IStaticTelemetryItemProvider[] providers)
        {
            _providers = providers;
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