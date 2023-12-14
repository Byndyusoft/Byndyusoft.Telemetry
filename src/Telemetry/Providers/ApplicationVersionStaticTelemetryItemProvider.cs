using Byndyusoft.AspNetCore.Mvc.Telemetry.Consts;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Providers
{
    public class ApplicationVersionStaticTelemetryItemProvider : IStaticTelemetryItemProvider
    {
        private readonly string _version;

        public ApplicationVersionStaticTelemetryItemProvider(string version)
        {
            _version = version;
        }

        public TelemetryItem[] GetTelemetryItems()
        {
            return new[]
            {
                new TelemetryItem(TelemetryItemNames.Version, _version)
            };
        }
    }
}