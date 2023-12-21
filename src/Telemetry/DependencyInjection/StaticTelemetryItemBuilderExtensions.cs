using Byndyusoft.Telemetry.Providers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class StaticTelemetryItemBuilderExtensions
    {
        public static StaticTelemetryItemBuilder WithBuildConfiguration(this StaticTelemetryItemBuilder builder)
        {
            return builder.WithProvider<BuildConfigurationStaticTelemetryItemProvider>();
        }

        public static StaticTelemetryItemBuilder WithAspNetCoreEnvironment(this StaticTelemetryItemBuilder builder)
        {
            return builder.WithProvider<AspNetCoreEnvironmentStaticTelemetryItemProvider>();
        }

        public static StaticTelemetryItemBuilder WithServiceName(this StaticTelemetryItemBuilder builder,
            string? serviceName = null)
        {
            return builder.WithProvider(new ServiceNameStaticTelemetryItemProvider(serviceName));
        }

        public static StaticTelemetryItemBuilder WithApplicationVersion(this StaticTelemetryItemBuilder builder,
            string version)
        {
            return builder.WithProvider(new ApplicationVersionStaticTelemetryItemProvider(version));
        }
    }
}