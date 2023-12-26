using Byndyusoft.Telemetry.Consts;
using Byndyusoft.Telemetry.Providers.Interface;

namespace Byndyusoft.Telemetry.Providers
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