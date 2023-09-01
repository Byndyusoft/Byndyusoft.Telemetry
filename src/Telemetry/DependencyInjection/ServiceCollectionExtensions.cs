using System;
using Byndyusoft.AspNetCore.Mvc.Telemetry;
using Byndyusoft.AspNetCore.Mvc.Telemetry.HostedServices;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelemetryRouter(
            this IServiceCollection services,
            Action<TelemetryRouterOptions> configureOptions)
        {
            services.Configure(configureOptions);

            services.PostConfigure<TelemetryRouterOptions>(options =>
            {
                foreach (var writerType in options.TelemetryWriterTypes)
                {
                    services.AddSingleton(writerType);
                }
            });

            services.AddSingleton<ITelemetryRouter, TelemetryRouter>();
            services.AddHostedService<InitializeTelemetryRouterHostedService>();

            return services;
        }
    }
}