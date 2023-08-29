using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Http
{
    public class ActivityTagWriter : ITelemetryWriter
    {
        public string WriterUniqueName => TelemetryActivityWriterUniqueNames.Tag;

        public void Write(TelemetryInfo[] telemetryInfos, bool isStaticData)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            foreach (var telemetryInfo in telemetryInfos)
                Write(telemetryInfo, activity);
        }

        private void Write(TelemetryInfo telemetryInfos, Activity activity)
        {
            foreach (var telemetryInfoItem in telemetryInfos)
                activity.SetTag(telemetryInfoItem.Key, telemetryInfoItem.Value);
        }
    }

    public class ActivityEventWriter : ITelemetryWriter
    {
        public string WriterUniqueName => TelemetryActivityWriterUniqueNames.Event;

        public void Write(TelemetryInfo[] telemetryInfos, bool isStaticData)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            foreach (var telemetryInfo in telemetryInfos) 
                Write(telemetryInfo, activity);
        }

        private void Write(TelemetryInfo telemetryInfo, Activity activity)
        {
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