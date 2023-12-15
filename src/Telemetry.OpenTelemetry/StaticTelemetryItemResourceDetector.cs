using System.Collections.Generic;
using System.Linq;
using OpenTelemetry.Resources;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.OpenTelemetry
{
    public class StaticTelemetryItemResourceDetector : IResourceDetector
    {
        public Resource Detect()
        {
            var attributes = StaticTelemetryItemsCollector
                .GetTelemetryItems()
                .Select(i => new KeyValuePair<string, object>(i.Name, i.Value ?? "n/a"))
                .ToArray();
            return new Resource(attributes);
        }
    }
}
