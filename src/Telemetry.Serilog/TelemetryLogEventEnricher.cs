using Serilog.Core;
using Serilog.Events;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog
{
    // TODO: Read from provider
    public class TelemetryLogEventEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var telemetryInfo in LogPropertyTelemetryDataAccessor.GetTelemetryData())
                Enrich(logEvent, propertyFactory, telemetryInfo);
        }

        private void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, TelemetryInfo telemetryInfo)
        {
            foreach (var telemetryInfoItem in telemetryInfo)
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(telemetryInfoItem.Key,
                    telemetryInfoItem.Value));
            }
        }
    }
}