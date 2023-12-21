using System.Collections.Generic;
using System.Diagnostics;

namespace Byndyusoft.Telemetry.OpenTelemetry
{
    public class ActivityTagEnricher
    {
        public static void Enrich(IEnumerable<TelemetryItem> telemetryItems)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            foreach (var telemetryItem in telemetryItems) 
                Enrich(activity, telemetryItem);
        }

        public static void Enrich(TelemetryItem telemetryItem)
        {
            var activity = Activity.Current;
            Enrich(activity, telemetryItem);
        }

        public static void Enrich(Activity? activity, TelemetryItem telemetryItem)
        {
            activity?.AddTag(telemetryItem.Name, telemetryItem.Value);
        }
    }
}