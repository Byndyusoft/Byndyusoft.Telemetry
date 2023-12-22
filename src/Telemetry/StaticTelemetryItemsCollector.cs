﻿using System.Collections.Generic;
using System.Linq;
using Byndyusoft.Telemetry.Providers.Interface;

namespace Byndyusoft.Telemetry
{
    public class StaticTelemetryItemsCollector
    {
        private static readonly List<TelemetryItem> TelemetryItems = new();

        public static IEnumerable<TelemetryItem> GetTelemetryItems() => TelemetryItems.AsEnumerable();

        internal static void CollectDataFrom(IStaticTelemetryItemProvider[] providers)
        {
            foreach (var provider in providers)
                CollectDataFrom(provider);
        }

        internal static void CollectDataFrom(IStaticTelemetryItemProvider provider)
        {
            TelemetryItems.AddRange(provider.GetTelemetryItems());
        }
    }
}