using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

    public class TelemetryRouterOptions
    {
        internal readonly List<Type> TelemetryWriterTypes = new();
        internal readonly List<(string ProviderUniqueName, string[] WriterUniqueNames)> Mapping = new();
        internal readonly List<IStaticTelemetryDataProvider> StaticTelemetryDataProviders = new();

        public void AddStaticTelemetryDataProvider(IStaticTelemetryDataProvider telemetryDataProvider)
        {
            StaticTelemetryDataProviders.Add(telemetryDataProvider);
        }

        public void AddWriter<T>() where T : ITelemetryWriter
        {
            TelemetryWriterTypes.Add(typeof(T));
        }

        public void AddRouting(string providerUniqueName, params string[] writerUniqueNames)
        {
            if (string.IsNullOrEmpty(providerUniqueName))
                throw new ArgumentNullException(nameof(providerUniqueName));

            Mapping.Add((providerUniqueName, writerUniqueNames));
        }
    }

    public class TelemetryRouter
    {
        private static ILogger<TelemetryRouter>? _logger;
        private static readonly Dictionary<string, ITelemetryWriter> TelemetryWritersByUniqueName = new();
        private static readonly IDictionary<string, HashSet<string>> WriterUniqueNamesByProviderUniqueName = new Dictionary<string, HashSet<string>>();
        private static readonly IDictionary<string, HashSet<string>> ProviderUniqueNamesByWriterUniqueName = new Dictionary<string, HashSet<string>>();
        private static readonly TelemetryData StaticTelemetryData = new();

        internal static void Initialize(
            ILogger<TelemetryRouter>? logger,
            TelemetryRouterOptions telemetryRouterOptions,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            foreach (var (providerUniqueName, writerUniqueNames) in telemetryRouterOptions.Mapping)
            {
                foreach (var writerUniqueName in writerUniqueNames)
                {
                    AddMapping(WriterUniqueNamesByProviderUniqueName, providerUniqueName, writerUniqueName);
                    AddMapping(ProviderUniqueNamesByWriterUniqueName, writerUniqueName, providerUniqueName);
                }
            }

            foreach (var telemetryWriterType in telemetryRouterOptions.TelemetryWriterTypes)
            {
                var telemetryWriter = (ITelemetryWriter)serviceProvider.GetRequiredService(telemetryWriterType);
                TelemetryWritersByUniqueName.Add(telemetryWriter.WriterUniqueName, telemetryWriter);
            }

            foreach (var telemetryInfoItemCollection in telemetryRouterOptions.StaticTelemetryDataProviders
                         .SelectMany(i => i.GetTelemetryData()))
                StaticTelemetryData.AddData(telemetryInfoItemCollection);
        }

        private static void AddMapping(IDictionary<string, HashSet<string>> mappingDictionary, string from, string to)
        {
            if (mappingDictionary.TryGetValue(from, out var existingHashSet) is false)
            {
                existingHashSet = new HashSet<string>();
                mappingDictionary.Add(from, existingHashSet);
            }

            existingHashSet.Add(to);
        }

        public static void WriteTelemetryInfo(TelemetryInfoItemCollection telemetryInfoItemCollection)
        {
            var providerUniqueName = telemetryInfoItemCollection.ProviderUniqueName;
            if (WriterUniqueNamesByProviderUniqueName.TryGetValue(providerUniqueName, out var writerUniqueNames) is
                false)
            {
                _logger?.LogWarning($"Не найдены писатели для данных от поставщика {providerUniqueName}");
                return;
            }

            foreach (var writerUniqueName in writerUniqueNames)
            {
                if (TelemetryWritersByUniqueName.TryGetValue(writerUniqueName, out var telemetryWriter) is false)
                    continue;

                telemetryWriter.Write(telemetryInfoItemCollection);
            }
        }

        public static TelemetryInfoItemCollection[] GetStaticTelemetryDataFor(string writerUniqueName)
        {
            if (ProviderUniqueNamesByWriterUniqueName.TryGetValue(writerUniqueName, out var providerUniqueNames) is
                false)
                return Array.Empty<TelemetryInfoItemCollection>();

            return providerUniqueNames
                .SelectMany(providerUniqueName => StaticTelemetryData.GetDataFor(providerUniqueName))
                .ToArray();
        }
    }

    internal class TelemetryData
    {
        private readonly ConcurrentDictionary<string, List<TelemetryInfoItemCollection>>
            _telemetryInfoByProviderUniqueName = new();

        public void AddData(TelemetryInfoItemCollection telemetryInfoItemCollection)
        {
            var providerUniqueName = telemetryInfoItemCollection.ProviderUniqueName;
            _telemetryInfoByProviderUniqueName.AddOrUpdate(
                providerUniqueName,
                _ => new List<TelemetryInfoItemCollection> { telemetryInfoItemCollection },
                (_, existingList) =>
                {
                    existingList.Add(telemetryInfoItemCollection);
                    return existingList;
                });
        }

        public IEnumerable<TelemetryInfoItemCollection> GetDataFor(string providerUniqueName)
        {
            if (_telemetryInfoByProviderUniqueName.TryGetValue(providerUniqueName, out var list))
                return list;

            return Enumerable.Empty<TelemetryInfoItemCollection>();
        }
    }

    public interface IStaticTelemetryDataProvider
    {
        TelemetryInfoItemCollection[] GetTelemetryData();
    }

    public class BuildConfigurationStaticTelemetryDataProvider : IStaticTelemetryDataProvider
    {
        public TelemetryInfoItemCollection[] GetTelemetryData()
        {
            const string buildEnvironmentKeyPrefix = "BUILD_";
            const string telemetryKeyPrefix = "build.";

            var telemetryInfoItemCollection = new TelemetryInfoItemCollection(
                TelemetryProviderUniqueNames.BuildConfiguration,
                "Build Configuration");

            var variables = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry variable in variables)
            {
                if (variable.Value is null)
                    continue;

                var value = variable.Value.ToString();
                if (string.IsNullOrEmpty(value))
                    continue;

                var property = variable.Key.ToString();
                if (property is not null && property.StartsWith(buildEnvironmentKeyPrefix))
                {
                    var key = property.Remove(0, buildEnvironmentKeyPrefix.Length);
                    key = $"{telemetryKeyPrefix}{key.ToLowerInvariant()}";
                    telemetryInfoItemCollection.Add(key, value);
                }
            }

            return new[] { telemetryInfoItemCollection };
        }
    }

    public static class TelemetryProviderUniqueNames
    {
        public static string BuildConfiguration => "BuildConfiguration";
    }

    public class InitializeTelemetryRouterHostedService : IHostedService
    {
        private readonly ILogger<TelemetryRouter> _logger;
        private readonly IOptions<TelemetryRouterOptions> _options;
        private readonly IServiceProvider _serviceProvider;

        public InitializeTelemetryRouterHostedService(
            ILogger<TelemetryRouter> logger,
            IOptions<TelemetryRouterOptions> options,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _options = options;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TelemetryRouter.Initialize(_logger, _options.Value, _serviceProvider);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}