using System;
using Byndyusoft.AspNetCore.Mvc.Telemetry;
using Byndyusoft.AspNetCore.Mvc.Telemetry.HostedServices;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Options;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Writers.Interfaces;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelemetryRouter(
            this IServiceCollection services,
            Action<TelemetryRouterOptions> configureOptions)
        {
            services.Configure<TelemetryRouterOptions>(configureOptions.Invoke);

            services.AddSingleton<ITelemetryRouter, TelemetryRouter>();
            services.AddHostedService<InitializeTelemetryRouterHostedService>();

            return services;
        }

        public static IServiceCollection AddTelemetryWriter<T>(this IServiceCollection services)
            where T : class, ITelemetryWriter
        {
            return services.AddSingleton<ITelemetryWriter, T>();
        }
    }
}