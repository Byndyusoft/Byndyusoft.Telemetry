﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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

    public interface ITelemetryRouter
    {
        void InitializeStaticTelemetryData();

        void WriteTelemetryInfo(TelemetryInfoItemCollection telemetryInfoItemCollection);

        TelemetryInfoItemCollection[] GetStaticTelemetryDataFor(string writerUniqueName);
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

    public class TelemetryRouter : ITelemetryRouter
    {
        private readonly ILogger<TelemetryRouter> _logger;
        private readonly Dictionary<string, ITelemetryWriter> _telemetryWritersByUniqueName = new();
        private readonly IDictionary<string, HashSet<string>> _writerUniqueNamesByProviderUniqueName = new Dictionary<string, HashSet<string>>();
        private readonly IDictionary<string, HashSet<string>> _providerUniqueNamesByWriterUniqueName = new Dictionary<string, HashSet<string>>();
        private readonly TelemetryData _staticTelemetryData = new();
        private bool _isStaticTelemetryDataInitialized = false;
        private object _lock = new();
        private readonly TelemetryRouterOptions _telemetryRouterOptions;

        public TelemetryRouter(
            ILogger<TelemetryRouter> logger,
            IOptions<TelemetryRouterOptions> options,
            IServiceProvider serviceProvider)
        {
            _logger = logger;

            _telemetryRouterOptions = options.Value;

            foreach (var (providerUniqueName, writerUniqueNames) in _telemetryRouterOptions.Mapping)
            {
                foreach (var writerUniqueName in writerUniqueNames)
                {
                    AddMapping(_writerUniqueNamesByProviderUniqueName, providerUniqueName, writerUniqueName);
                    AddMapping(_providerUniqueNamesByWriterUniqueName, writerUniqueName, providerUniqueName);
                }
            }

            foreach (var telemetryWriterType in _telemetryRouterOptions.TelemetryWriterTypes)
            {
                var telemetryWriter = (ITelemetryWriter)serviceProvider.GetRequiredService(telemetryWriterType);
                _telemetryWritersByUniqueName.Add(telemetryWriter.WriterUniqueName, telemetryWriter);
            }
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

        public void InitializeStaticTelemetryData()
        {
            if (_isStaticTelemetryDataInitialized)
                return;

            lock (_lock)
            {
                if (_isStaticTelemetryDataInitialized)
                    return;

                foreach (var telemetryInfoItemCollection in _telemetryRouterOptions.StaticTelemetryDataProviders
                             .SelectMany(i => i.GetTelemetryData()))
                    _staticTelemetryData.AddData(telemetryInfoItemCollection);

                _isStaticTelemetryDataInitialized = true;
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

        public TelemetryInfoItemCollection[] GetStaticTelemetryDataFor(string writerUniqueName)
        {
            if (_providerUniqueNamesByWriterUniqueName.TryGetValue(writerUniqueName, out var providerUniqueNames) is
                false)
                return Array.Empty<TelemetryInfoItemCollection>();

            return providerUniqueNames
                .SelectMany(providerUniqueName => _staticTelemetryData.GetDataFor(providerUniqueName))
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
            const string buildKeyPrefix = "BUILD_";

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
                if (property is not null && property.StartsWith(buildKeyPrefix))
                {
                    var key = property.Remove(0, buildKeyPrefix.Length);
                    telemetryInfoItemCollection.Add(key.ToLowerInvariant(), value);
                }
            }

            return new[] { telemetryInfoItemCollection };
        }
    }

    public static class TelemetryProviderUniqueNames
    {
        public static string BuildConfiguration => "BuildConfiguration";
    }

    public class InitializeStaticTelemetryDataHostedService : IHostedService
    {
        private readonly ITelemetryRouter _telemetryRouter;

        public InitializeStaticTelemetryDataHostedService(ITelemetryRouter telemetryRouter)
        {
            _telemetryRouter = telemetryRouter;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _telemetryRouter.InitializeStaticTelemetryData();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}