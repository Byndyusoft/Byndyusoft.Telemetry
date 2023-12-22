// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static StaticTelemetryItemBuilder ConfigureStaticTelemetryItemCollector(this IServiceCollection services)
        {
            return new StaticTelemetryItemBuilder();
        }
    }
}