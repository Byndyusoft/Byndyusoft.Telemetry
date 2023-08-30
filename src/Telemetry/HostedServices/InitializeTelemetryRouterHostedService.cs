using System;
using System.Threading;
using System.Threading.Tasks;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.HostedServices
{
    public class InitializeTelemetryRouterHostedService : IHostedService
    {
        private readonly IOptions<TelemetryRouterOptions> _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly TelemetryRouter _telemetryRouter;

        public InitializeTelemetryRouterHostedService(
            IOptions<TelemetryRouterOptions> options,
            IServiceProvider serviceProvider,
            ITelemetryRouter telemetryRouter)
        {
            _options = options;
            _serviceProvider = serviceProvider;
            _telemetryRouter = (TelemetryRouter)telemetryRouter;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _telemetryRouter.Initialize(_options.Value, _serviceProvider);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}