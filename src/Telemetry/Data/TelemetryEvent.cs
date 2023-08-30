namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Data
{
    public class TelemetryEvent
    {
        public TelemetryEvent(string eventName, params TelemetryInfo[] telemetryInfos)
        {
            EventName = eventName;
            TelemetryInfos = telemetryInfos;
        }

        public string EventName { get; }

        public TelemetryInfo[] TelemetryInfos { get; }
    }
}