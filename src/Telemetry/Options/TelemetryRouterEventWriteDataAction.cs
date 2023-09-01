namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Options
{
    public class TelemetryRouterEventWriteDataAction
    {
        public TelemetryRouterEventWriteDataAction(
            bool isStatic,
            string telemetryUniqueName,
            params string[] telemetryWriterUniqueNames)
        {
            IsStatic = isStatic;
            TelemetryUniqueName = telemetryUniqueName;
            TelemetryWriterUniqueNames = telemetryWriterUniqueNames;
        }

        public bool IsStatic { get; }

        public string TelemetryUniqueName { get; }

        public string[] TelemetryWriterUniqueNames { get; }
    }
}