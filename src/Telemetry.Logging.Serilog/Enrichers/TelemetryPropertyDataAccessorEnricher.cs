using Serilog.Core;
using Serilog.Events;

namespace Byndyusoft.Telemetry.Logging.Serilog.Enrichers
{
    public class TelemetryPropertyDataAccessorEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var telemetryItem in LogPropertyDataAccessor.GetTelemetryItems())
                Enrich(logEvent, propertyFactory, telemetryItem);
        }

        private void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, TelemetryItem telemetryItem)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(telemetryItem.Name.Replace('.', '_'), telemetryItem.Value));
        }
    }
}