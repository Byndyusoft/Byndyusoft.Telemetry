using Byndyusoft.AspNetCore.Mvc.Telemetry.HostedServices;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static StaticTelemetryItemBuilder AddStaticTelemetryItemCollector(this IServiceCollection services)
        {
            services.AddHostedService<CollectStaticTelemetryHostedService>();
            return new StaticTelemetryItemBuilder(services);
        }
    }
}