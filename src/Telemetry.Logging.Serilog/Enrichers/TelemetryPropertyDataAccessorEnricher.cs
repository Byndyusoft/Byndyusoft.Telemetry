namespace Byndyusoft.Telemetry.Logging.Serilog.Enrichers
{
    using Abstraction;
    using global::Serilog.Core;
    using global::Serilog.Events;

    public class TelemetryPropertyDataAccessorEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var telemetryItem in LogPropertyDataAccessor.GetTelemetryItems())
                Enrich(logEvent, propertyFactory, telemetryItem);
        }

        private void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, TelemetryItem telemetryItem)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(telemetryItem.Name.Replace('.', '_'),
                                                                        telemetryItem.Value));
        }
    }
}