using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Byndyusoft.Telemetry.Logging
{
    public class LogPropertyDataAccessor
    {
        private static readonly AsyncLocal<EventDataHolder> EventDataCurrent = new();

        public static void InitAsyncContext()
        {
            EventDataCurrent.Value ??= new EventDataHolder();
        }

        public static void AddTelemetryItem(string name, object? value)
        {
            AddTelemetryItem(new TelemetryItem(name, value));
        }

        public static void AddTelemetryItem(TelemetryItem telemetryItem)
        {
            InitAsyncContext();
            EventDataCurrent.Value.EventData.Add(telemetryItem);
        }

        public static void AddTelemetryItems(TelemetryItem[] telemetryItems)
        {
            InitAsyncContext();
            EventDataCurrent.Value.EventData.AddRange(telemetryItems);
        }

        public static IEnumerable<TelemetryItem> GetTelemetryItems()
        {
            if (EventDataCurrent.Value is null)
                return Array.Empty<TelemetryItem>();

            return EventDataCurrent.Value.EventData.AsEnumerable();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class EventDataHolder
        {
            public readonly List<TelemetryItem> EventData = new();
        }
    }
}