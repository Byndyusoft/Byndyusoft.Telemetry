using System;
using System.Collections;
using System.Collections.Generic;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Data
{
    public class TelemetryInfo : IEnumerable<TelemetryInfoItem>
    {
        private readonly List<TelemetryInfoItem> _telemetryInfoItems = new();

        public TelemetryInfo(string telemetryUniqueName, string message)
        {
            TelemetryUniqueName = telemetryUniqueName ?? throw new ArgumentNullException(nameof(telemetryUniqueName));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string TelemetryUniqueName { get; }

        public string Message { get; }

        public IEnumerator<TelemetryInfoItem> GetEnumerator()
        {
            return _telemetryInfoItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TelemetryInfoItem telemetryInfoItem)
        {
            _telemetryInfoItems.Add(telemetryInfoItem);
        }

        public void Add(string key, object? value)
        {
            var telemetryInfoItem = new TelemetryInfoItem(key, value);
            Add(telemetryInfoItem);
        }
    }
}