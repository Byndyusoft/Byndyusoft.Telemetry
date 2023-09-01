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

        public TelemetryRouterEventOptions AddWriteDataAction(TelemetryRouterEventWriteDataAction writeDataAction)
        {
            _writeDataActions.Add(writeDataAction);
            return this;
        }

        public TelemetryRouterEventWriteDataActionBuilder WriteStaticData(string telemetryInfoName)
        {
            return new TelemetryRouterEventWriteDataActionBuilder(this, true, telemetryInfoName);
        }

        public TelemetryRouterEventWriteDataActionBuilder WriteEventData(string telemetryInfoName)
        {
            return new TelemetryRouterEventWriteDataActionBuilder(this, false, telemetryInfoName);
        }
    }
}