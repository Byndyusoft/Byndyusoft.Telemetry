namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Options
{
    public class TelemetryRouterEventWriteDataActionBuilder
    {
        private readonly bool _isStatic;
        private readonly string _telemetryInfoName;
        private readonly TelemetryRouterEventOptions _telemetryRouterEventOptions;

        public TelemetryRouterEventWriteDataActionBuilder(
            TelemetryRouterEventOptions telemetryRouterEventOptions,
            bool isStatic,
            string telemetryInfoName)
        {
            _isStatic = isStatic;
            _telemetryInfoName = telemetryInfoName;
            _telemetryRouterEventOptions = telemetryRouterEventOptions;
        }

        public TelemetryRouterEventOptions To(params string[] telemetryWriterUniqueNames)
        {
            var writeDataAction =
                new TelemetryRouterEventWriteDataAction(_isStatic, _telemetryInfoName, telemetryWriterUniqueNames);
            return _telemetryRouterEventOptions.AddWriteDataAction(writeDataAction);
        }
    }
}