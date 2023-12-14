using System.Reflection;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Consts;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Providers
{
    public class ServiceNameStaticTelemetryItemProvider : IStaticTelemetryItemProvider
    {
        private readonly string? _serviceName;

        public ServiceNameStaticTelemetryItemProvider(string? serviceName)
        {
            _serviceName = string.IsNullOrWhiteSpace(serviceName)
                ? GetAssemblyName()
                : serviceName;
        }

        private string GetAssemblyName() => Assembly.GetEntryAssembly()?.GetName().Name ?? "";

        public TelemetryItem[] GetTelemetryItems()
        {
            return new[]
            {
                new TelemetryItem(TelemetryItemNames.ServiceName, _serviceName)
            };
        }
    }
}