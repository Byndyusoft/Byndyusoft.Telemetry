using System.Collections.Generic;
using System.Linq;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Options
{
    public class TelemetryRouterEventOptions
    {
        private readonly List<TelemetryRouterEventWriteDataAction> _writeDataActions = new();

        public TelemetryRouterEventOptions(string eventName)
        {
            EventName = eventName;
        }

        public string EventName { get; }

        public IEnumerable<TelemetryRouterEventWriteDataAction> EnumerationWriteDataActions()
        {
            return _writeDataActions.AsEnumerable();
        }

        private void AddWriteDataAction(TelemetryRouterEventWriteDataAction writeDataAction)
        {
            _writeDataActions.Add(writeDataAction);
        }

        // TODO: Consider using builder
        public TelemetryRouterEventOptions WriteStaticData(string telemetryInfoName, params string[] telemetryWriterUniqueNames)
        {
            var writeDataAction =
                new TelemetryRouterEventWriteDataAction(true, telemetryInfoName, telemetryWriterUniqueNames);
            AddWriteDataAction(writeDataAction);
            return this;
        }

        public TelemetryRouterEventOptions WriteEventData(string telemetryInfoName, params string[] telemetryWriterUniqueNames)
        {
            var writeDataAction =
                new TelemetryRouterEventWriteDataAction(false, telemetryInfoName, telemetryWriterUniqueNames);
            AddWriteDataAction(writeDataAction);
            return this;
        }
    }
}