using Byndyusoft.AspNetCore.Mvc.Telemetry.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogPropertyDataAccessor(this IServiceCollection services)
        {
            return services.AddSingleton<LogPropertyDataAccessor>();
        }
    }
}