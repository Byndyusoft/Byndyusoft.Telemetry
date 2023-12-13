using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Logging
{
    public class LogPropertyDataAccessor
    {
        private static readonly AsyncLocal<EventDataHolder> EventDataCurrent = new();

        internal static void AddTelemetryInfos(PropertyDataItem[] propertyDataItems)
        {
            EventDataCurrent.Value ??= new EventDataHolder();
            EventDataCurrent.Value.EventData.AddRange(propertyDataItems);
        }

        public static IEnumerable<PropertyDataItem> GetTelemetryData()
        {
            if (EventDataCurrent.Value is null)
                return Array.Empty<PropertyDataItem>();

            return EventDataCurrent.Value.EventData.AsEnumerable();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class EventDataHolder
        {
            public readonly List<PropertyDataItem> EventData = new();
        }
    }
}