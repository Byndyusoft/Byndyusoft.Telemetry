using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry
{
    public class TelemetryInfoItem
    {
        public TelemetryInfoItem(string key, object? value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }

        public object? Value { get; }
    }

    public class TelemetryInfoItemCollection : IEnumerable<TelemetryInfoItem>
    {
        private readonly List<TelemetryInfoItem> _telemetryInfoItems;

        public TelemetryInfoItemCollection(params TelemetryInfoItem[] telemetryInfoItems)
        {
            _telemetryInfoItems = telemetryInfoItems.ToList();
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

        public IEnumerator<TelemetryInfoItem> GetEnumerator()
        {
            return _telemetryInfoItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class TelemetryWriterNames
    {
        public static string Log => "Log";
    }

    public interface ITelemetryWriter
    {
        void Write(TelemetryInfoItemCollection telemetryInfoItemCollection);
    }

    public class LogWriter : ITelemetryWriter
    {
        private readonly ILogger<LogWriter> _logger;

        public LogWriter(ILogger<LogWriter> logger)
        {
            _logger = logger;
        }

        public void Write(TelemetryInfoItemCollection telemetryInfoItemCollection)
        {
            var messageBuilder = new StringBuilder("Telemetry Message: ");
            var arguments = new List<object>();

            foreach (var telemetryInfoItem in telemetryInfoItemCollection)
            {
                messageBuilder.Append($" {telemetryInfoItem.Key} = {{{telemetryInfoItem.Key}}};");
                arguments.Add(telemetryInfoItem.Value);
            }

            _logger.LogInformation(messageBuilder.ToString(), arguments.ToArray());
        }
    }
}