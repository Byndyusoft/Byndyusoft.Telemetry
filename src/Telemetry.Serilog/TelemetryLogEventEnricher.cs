using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog
{
    public class TelemetryLogEventEnricher : ILogEventEnricher
    {
        private readonly ITelemetryRouter _telemetryRouter;

        public TelemetryLogEventEnricher(ITelemetryRouter telemetryRouter)
        {
            _telemetryRouter = telemetryRouter;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var telemetryData = _telemetryRouter.GetStaticTelemetryDataFor(SerilogTelemetryWriterUniqueNames.Property);
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