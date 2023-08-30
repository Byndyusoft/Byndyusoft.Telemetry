using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Data;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry
{
    internal class TelemetryInfoStorage
    {
        private readonly ConcurrentDictionary<string, List<TelemetryInfo>>
            _telemetryInfosByUniqueName = new();

        public void AddData(TelemetryInfo telemetryInfo)
        {
            var telemetryUniqueName = telemetryInfo.TelemetryUniqueName;
            _telemetryInfosByUniqueName.AddOrUpdate(
                telemetryUniqueName,
                _ => new List<TelemetryInfo> { telemetryInfo },
                (_, existingList) =>
                {
                    existingList.Add(telemetryInfo);
                    return existingList;
                });
        }

        public IEnumerable<TelemetryInfo> GetData(string telemetryUniqueName)
        {
            if (_telemetryInfosByUniqueName.TryGetValue(telemetryUniqueName, out var list))
                return list;

            return Enumerable.Empty<TelemetryInfo>();
        }
    }
}