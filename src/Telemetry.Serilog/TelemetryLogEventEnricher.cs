using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog
{
    // TODO: Read from provider
    public class TelemetryLogEventEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var telemetryData = TelemetryRouter.GetStaticTelemetryInfoFor(SerilogTelemetryWriterUniqueNames.Property);
            foreach (var telemetryInfoItem in telemetryData.SelectMany(i => i))
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(telemetryInfoItem.Key,
                    telemetryInfoItem.Value));
            }
        }
    }

    public static class SerilogTelemetryWriterUniqueNames
    {
        public static string Property => "SerilogProperty";
    }
}