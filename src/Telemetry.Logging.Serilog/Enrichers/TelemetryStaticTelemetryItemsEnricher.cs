﻿namespace Byndyusoft.Telemetry.Logging.Serilog.Enrichers
{
    using Abstraction;
    using Extensions;
    using global::Serilog.Core;
    using global::Serilog.Events;

    public class TelemetryStaticTelemetryItemsEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var telemetryItem in StaticTelemetryItemsCollector.GetTelemetryItems())
                Enrich(logEvent, propertyFactory, telemetryItem);
        }

        private void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, TelemetryItem telemetryItem)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(telemetryItem.GetLogEventPropertyName(),
                                                                        telemetryItem.Value));
        }
    }
}