using Byndyusoft.AspNetCore.Mvc.Telemetry.OpenTelemetry;

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Resources
{
    public static class ResourceBuilderExtensions
    {
        public static ResourceBuilder AddStaticTelemetryItems(this ResourceBuilder builder)
        {
            return builder.AddDetector(new StaticTelemetryItemResourceDetector());
        }
    }
}