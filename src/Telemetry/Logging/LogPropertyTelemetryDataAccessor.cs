using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Data;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Logging
{
    public static class LogPropertyTelemetryDataAccessor
    {
        private static readonly List<TelemetryInfo> StaticData = new();
        private static readonly AsyncLocal<EventDataHolder> EventDataCurrent = new();

        internal static void AddTelemetryInfos(TelemetryInfo[] telemetryInfos, bool isStaticData)
        {
            if (isStaticData)
                StaticData.AddRange(telemetryInfos);
            else
            {
                EventDataCurrent.Value ??= new EventDataHolder();
                EventDataCurrent.Value.EventData.AddRange(telemetryInfos);
            }
        }

        public static IEnumerable<TelemetryInfo> GetTelemetryData()
        {
            if (EventDataCurrent.Value is null)
                return StaticData.AsEnumerable();

            return StaticData.Concat(EventDataCurrent.Value.EventData);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class EventDataHolder
        {
            public readonly List<TelemetryInfo> EventData = new();
        }
    }
}