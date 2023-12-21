using Byndyusoft.Telemetry;
using Byndyusoft.Telemetry.HostedServices;

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

        public static IServiceCollection AddObjectTelemetryItemCollector(this IServiceCollection services)
        {
            return services.AddSingleton<ObjectTelemetryItemsCollector>();
        }
    }
}