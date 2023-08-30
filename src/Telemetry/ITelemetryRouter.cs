using Byndyusoft.AspNetCore.Mvc.Telemetry.Data;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry
{
    public interface ITelemetryRouter
    {
        void ProcessTelemetryEvent(TelemetryEvent telemetryEvent);
    }
}