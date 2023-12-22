using Byndyusoft.Telemetry;
using Byndyusoft.Telemetry.Providers.Interface;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public class StaticTelemetryItemBuilder
    {
        public StaticTelemetryItemBuilder WithProvider<T>(T provider)
            where T : IStaticTelemetryItemProvider
        {
            StaticTelemetryItemsCollector.CollectDataFrom(provider);
            return this;
        }
    }
}