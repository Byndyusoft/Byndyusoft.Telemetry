using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public class StaticTelemetryItemBuilder
    {
        private readonly IServiceCollection _services;

        public StaticTelemetryItemBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public StaticTelemetryItemBuilder WithProvider<T>()
            where T : IStaticTelemetryItemProvider
        {
            _services.AddSingleton(typeof(IStaticTelemetryItemProvider), typeof(T));
            return this;
        }

        public StaticTelemetryItemBuilder WithProvider<T>(T provider)
        where T : IStaticTelemetryItemProvider
        {
            _services.AddSingleton<IStaticTelemetryItemProvider>(provider);
            return this;
        }
    }
}