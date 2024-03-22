

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Resources
{
    using Byndyusoft.Telemetry.OpenTelemetry;

    public static class ResourceBuilderExtensions
    {
        public static ResourceBuilder AddStaticTelemetryItems(this ResourceBuilder builder)
        {
            return builder.AddDetector(new StaticTelemetryItemResourceDetector());
        }
    }
}