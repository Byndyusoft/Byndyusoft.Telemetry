using System.Collections.Generic;
using System.Linq;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry
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

        private static void CollectDataFrom(IStaticTelemetryItemProvider provider)
        {
            TelemetryItems.AddRange(provider.GetTelemetryItems());
        }
    }
}