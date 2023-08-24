using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Http
{
    public class ActivityTagWriter : ITelemetryWriter
    {
        public string WriterUniqueName => TelemetryActivityWriterUniqueNames.Tag;

        public void Write(TelemetryInfoItemCollection telemetryInfoItemCollection)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            foreach (var telemetryInfoItem in telemetryInfoItemCollection)
                activity.SetTag(telemetryInfoItem.Key, telemetryInfoItem.Value);
        }
    }

    public class ActivityEventWriter : ITelemetryWriter
    {
        public string WriterUniqueName => TelemetryActivityWriterUniqueNames.Event;

        public void Write(TelemetryInfoItemCollection telemetryInfoItemCollection)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            var activityTagsCollection =
                new ActivityTagsCollection(
                    telemetryInfoItemCollection.Select(i => new KeyValuePair<string, object?>(i.Key, i.Value)));
            var activityEvent = new ActivityEvent(telemetryInfoItemCollection.Message, tags: activityTagsCollection);
            activity.AddEvent(activityEvent);
        }
    }

    public static class TelemetryActivityWriterUniqueNames
    {
        public static string Tag => "ActivityTag";

        public static string Event => "ActivityEvent";
    }
}