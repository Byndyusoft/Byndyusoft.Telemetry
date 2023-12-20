using System.Collections.Generic;
using System.Diagnostics;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.OpenTelemetry
{
    public class ActivityTagEnricher
    {
        public static void Enrich(IEnumerable<TelemetryItem> telemetryItems)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            foreach (var telemetryItem in telemetryItems) 
                activity.AddTag(telemetryItem.Name, telemetryItem.Value);
        }
    }
}