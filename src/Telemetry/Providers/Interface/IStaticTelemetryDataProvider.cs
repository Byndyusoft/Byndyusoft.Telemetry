using Byndyusoft.AspNetCore.Mvc.Telemetry.Data;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface
{
    public interface IStaticTelemetryDataProvider
    {
        TelemetryInfo[] GetTelemetryData();
    }
}