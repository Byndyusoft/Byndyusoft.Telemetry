using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Http
{
    public class ActivityTagWriter : ITelemetryWriter
    {
        public string WriterUniqueName => TelemetryActivityWriterUniqueNames.Tag;

        public void Write(TelemetryInfo telemetryInfo, bool isStaticData)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            foreach (var telemetryInfoItem in telemetryInfo)
                activity.SetTag(telemetryInfoItem.Key, telemetryInfoItem.Value);
        }
    }

    public class ActivityEventWriter : ITelemetryWriter
    {
        public string WriterUniqueName => TelemetryActivityWriterUniqueNames.Event;

        public void Write(TelemetryInfo telemetryInfo, bool isStaticData)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            var activityTagsCollection =
                new ActivityTagsCollection(
                    telemetryInfo.Select(i => new KeyValuePair<string, object?>(i.Key, i.Value)));
            var activityEvent = new ActivityEvent(telemetryInfo.Message, tags: activityTagsCollection);
            activity.AddEvent(activityEvent);
        }
    }

    public static class TelemetryActivityWriterUniqueNames
    {
        public static string Tag => "ActivityTag";

        public static string Event => "ActivityEvent";
    }
}