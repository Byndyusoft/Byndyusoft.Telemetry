﻿using System.Collections.Generic;
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
            Enrich(Activity.Current, telemetryItem);
        }

        public static void Enrich(Activity? activity, TelemetryItem telemetryItem)
        {
            activity?.AddTag(telemetryItem.Name, telemetryItem.Value);
        }

        public static void Enrich(string name, object? value)
        {
            Enrich(Activity.Current, name, value);
        }

        public static void Enrich(Activity? activity, string name, object? value)
        {
            activity?.AddTag(name, value);
        }
    }
}