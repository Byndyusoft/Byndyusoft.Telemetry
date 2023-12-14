using Byndyusoft.AspNetCore.Mvc.Telemetry.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog
{
    public class TelemetryLogEventEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var propertyDataItem in LogPropertyDataAccessor.GetPropertyDataItems())
                Enrich(logEvent, propertyFactory, propertyDataItem);
        }

        private void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, PropertyDataItem propertyDataItem)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(propertyDataItem.Name.Replace('.', '_'), propertyDataItem.Value));
        }
    }
}