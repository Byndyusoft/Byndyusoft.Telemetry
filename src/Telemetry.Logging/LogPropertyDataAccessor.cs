using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Logging
{
    public class LogPropertyDataAccessor
    {
        private static readonly AsyncLocal<EventDataHolder> EventDataCurrent = new();

        public static void AddPropertyDataItem(string name, object? value)
        {
            AddPropertyDataItem(new PropertyDataItem(name, value));
        }

        public static void AddPropertyDataItem(PropertyDataItem propertyDataItem)
        {
            EventDataCurrent.Value ??= new EventDataHolder();
            EventDataCurrent.Value.EventData.Add(propertyDataItem);
        }

        public static void AddPropertyDataItems(PropertyDataItem[] propertyDataItems)
        {
            EventDataCurrent.Value ??= new EventDataHolder();
            EventDataCurrent.Value.EventData.AddRange(propertyDataItems);
        }

        public static IEnumerable<PropertyDataItem> GetPropertyDataItems()
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