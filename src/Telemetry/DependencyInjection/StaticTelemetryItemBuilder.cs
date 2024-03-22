

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    using Byndyusoft.Telemetry;
    using Byndyusoft.Telemetry.Abstraction.Interfaces;

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