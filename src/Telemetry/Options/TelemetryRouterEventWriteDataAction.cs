namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Options
{
    public class TelemetryRouterEventWriteDataAction
    {
        public TelemetryRouterEventWriteDataAction(
            bool isStatic,
            string telemetryInfoName,
            params string[] telemetryWriterUniqueNames)
        {
            IsStatic = isStatic;
            TelemetryInfoName = telemetryInfoName;
            TelemetryWriterUniqueNames = telemetryWriterUniqueNames;
        }

        public bool IsStatic { get; }

        public string TelemetryInfoName { get; }

        public string[] TelemetryWriterUniqueNames { get; }
    }
}