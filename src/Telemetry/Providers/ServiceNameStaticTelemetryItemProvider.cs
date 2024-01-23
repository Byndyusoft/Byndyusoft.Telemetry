namespace Byndyusoft.Telemetry.Providers
{
    using System.Reflection;
    using Consts;
    using Interface;

    public class ServiceNameStaticTelemetryItemProvider : IStaticTelemetryItemProvider
    {
        private readonly string? _serviceName;

        public ServiceNameStaticTelemetryItemProvider(string? serviceName)
        {
            _serviceName = string.IsNullOrWhiteSpace(serviceName)
                               ? GetAssemblyName()
                               : serviceName;
        }

        public TelemetryItem[] GetTelemetryItems()
        {
            return new[]
                       {
                           new TelemetryItem(TelemetryItemNames.ServiceName, _serviceName)
                       };
        }

        private string GetAssemblyName() => Assembly.GetEntryAssembly()?.GetName().Name ?? "";
    }
}