using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private readonly List<TelemetryInfoItem> _telemetryInfoItems = new();

        public TelemetryInfoItemCollection(string providerUniqueName, string message)
        {
            ProviderUniqueName = providerUniqueName ?? throw new ArgumentNullException(nameof(providerUniqueName));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string ProviderUniqueName { get; }

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

    public static class TelemetryWriterNames
    {
        public static string Log => "Log";
    }

    public interface ITelemetryWriter
    {
        string WriterUniqueName { get; }

        void Write(TelemetryInfoItemCollection telemetryInfoItemCollection);
    }

    public class LogWriter : ITelemetryWriter
    {
        private readonly ILogger<LogWriter> _logger;

        public LogWriter(ILogger<LogWriter> logger)
        {
            _logger = logger;
        }

        public string WriterUniqueName => TelemetryWriterUniqueNames.Log;

        public void Write(TelemetryInfoItemCollection telemetryInfoItemCollection)
        {
            var messageBuilder = new StringBuilder($"{telemetryInfoItemCollection.Message}: ");
            var arguments = new List<object?>();

            foreach (var telemetryInfoItem in telemetryInfoItemCollection)
            {
                messageBuilder.Append($" {telemetryInfoItem.Key} = {{{telemetryInfoItem.Key}}};");
                arguments.Add(telemetryInfoItem.Value);
            }

            _logger.LogInformation(messageBuilder.ToString(), arguments.ToArray());
        }
    }

    public static class TelemetryWriterUniqueNames
    {
        public static string Log => "Log";
    }

    public interface ITelemetryRouter
    {
        void WriteTelemetryInfo(TelemetryInfoItemCollection telemetryInfoItemCollection);
    }

    public class TelemetryRouterOptions
    {
        internal readonly ICollection<Type> TelemetryWriterTypes = new List<Type>();
        internal readonly IDictionary<string, HashSet<string>> WriterUniqueNamesByProviderUniqueName = new Dictionary<string, HashSet<string>>();

        public void AddWriter<T>() where T : ITelemetryWriter
        {
            TelemetryWriterTypes.Add(typeof(T));
        }

        public void AddRouting(string providerUniqueName, params string[] writerUniqueNames)
        {
            if (string.IsNullOrEmpty(providerUniqueName))
                throw new ArgumentNullException(nameof(providerUniqueName));

            if (WriterUniqueNamesByProviderUniqueName.TryGetValue(providerUniqueName, out var existingWriterNames) is
                false)
            {
                existingWriterNames = new HashSet<string>();
                WriterUniqueNamesByProviderUniqueName.Add(providerUniqueName, existingWriterNames);
            }

            foreach (var writerUniqueName in writerUniqueNames)
                existingWriterNames.Add(writerUniqueName);
        }
    }

    public class TelemetryRouter : ITelemetryRouter
    {
        private readonly ILogger<TelemetryRouter> _logger;
        private readonly Dictionary<string, ITelemetryWriter> _telemetryWritersByUniqueName = new();
        private readonly IDictionary<string, HashSet<string>> _writerUniqueNamesByProviderUniqueName;

        public TelemetryRouter(
            ILogger<TelemetryRouter> logger,
            IOptions<TelemetryRouterOptions> options,
            IServiceProvider serviceProvider)
        {
            _logger = logger;

            var telemetryRouterOptions = options.Value;
            _writerUniqueNamesByProviderUniqueName = telemetryRouterOptions.WriterUniqueNamesByProviderUniqueName;

            foreach (var telemetryWriterType in telemetryRouterOptions.TelemetryWriterTypes)
            {
                var telemetryWriter = (ITelemetryWriter)serviceProvider.GetRequiredService(telemetryWriterType);
                _telemetryWritersByUniqueName.Add(telemetryWriter.WriterUniqueName, telemetryWriter);
            }
        }

        public void WriteTelemetryInfo(TelemetryInfoItemCollection telemetryInfoItemCollection)
        {
            var providerUniqueName = telemetryInfoItemCollection.ProviderUniqueName;
            if (_writerUniqueNamesByProviderUniqueName.TryGetValue(providerUniqueName, out var writerUniqueNames) is
                false)
            {
                _logger.LogWarning($"Не найдены писатели для данных от поставщика {providerUniqueName}");
                return;
            }

            foreach (var writerUniqueName in writerUniqueNames)
            {
                if (_telemetryWritersByUniqueName.TryGetValue(writerUniqueName, out var telemetryWriter) is false)
                    continue;

                telemetryWriter.Write(telemetryInfoItemCollection);
            }
        }
    }
}