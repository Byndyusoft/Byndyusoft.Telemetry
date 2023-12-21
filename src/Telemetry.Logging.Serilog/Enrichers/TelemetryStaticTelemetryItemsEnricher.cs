using Serilog.Core;
using Serilog.Events;

namespace Byndyusoft.Telemetry.Logging.Serilog.Enrichers
{
    public class TelemetryStaticTelemetryItemsEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var telemetryItem in StaticTelemetryItemsCollector.GetTelemetryItems())
                Enrich(logEvent, propertyFactory, telemetryItem);
        }

        private void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, TelemetryItem telemetryItem)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(telemetryItem.Name.Replace('.', '_'), telemetryItem.Value));
        }
    }
}