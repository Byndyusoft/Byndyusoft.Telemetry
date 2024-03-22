namespace Byndyusoft.Telemetry.Logging.Serilog.Extensions
{
    using Abstraction;

    public static class TelemetryItemExtensions
    {
        public static string GetLogEventPropertyName(this TelemetryItem telemetryItem)
        {
            return telemetryItem.Name.Replace('.', '_');
        }
    }
}