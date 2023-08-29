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

    public interface ITelemetryWriter
    {
        string WriterUniqueName { get; }

        void Write(TelemetryInfo[] telemetryInfos, bool isStaticData);
    }

    public class LogWriter : ITelemetryWriter
    {
        private readonly ILogger<LogWriter> _logger;

        public LogWriter(ILogger<LogWriter> logger)
        {
            _logger = logger;
        }

        public string WriterUniqueName => TelemetryWriterUniqueNames.Log;

        public void Write(TelemetryInfo[] telemetryInfos, bool isStaticData)
        {
            foreach (var telemetryInfo in telemetryInfos) 
                Write(telemetryInfo);
        }

        public void Write(TelemetryInfo telemetryInfo)
        {
            var messageBuilder = new StringBuilder($"{telemetryInfo.Message}: ");
            var arguments = new List<object?>();

            foreach (var telemetryInfoItem in telemetryInfo)
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

        public static string LogProperty => "LogProperty";
    }

    public class TelemetryRouterOptions
    {
        internal readonly List<TelemetryRouterEventOptions> EventOptions = new();
        internal readonly List<IStaticTelemetryDataProvider> StaticTelemetryDataProviders = new();
        internal readonly List<Type> TelemetryWriterTypes = new();

        public void AddStaticTelemetryDataProvider(IStaticTelemetryDataProvider telemetryDataProvider)
        {
            StaticTelemetryDataProviders.Add(telemetryDataProvider);
        }

        public void AddWriter<T>() where T : ITelemetryWriter
        {
            TelemetryWriterTypes.Add(typeof(T));
        }

        public void AddEvent(string eventName, Action<TelemetryRouterEventOptions> configureOptions)
        {
            var telemetryRouterEventOptions = new TelemetryRouterEventOptions(eventName);
            configureOptions.Invoke(telemetryRouterEventOptions);

            EventOptions.Add(telemetryRouterEventOptions);
        }
    }

    public class TelemetryRouterEventOptions
    {
        private readonly List<TelemetryRouterEventWriteDataAction> _writeDataActions = new();

        public TelemetryRouterEventOptions(string eventName)
        {
            EventName = eventName;
        }

        public string EventName { get; }

        public IEnumerable<TelemetryRouterEventWriteDataAction> EnumerationWriteDataActions()
        {
            return _writeDataActions.AsEnumerable();
        }

        private void AddWriteDataAction(TelemetryRouterEventWriteDataAction writeDataAction)
        {
            _writeDataActions.Add(writeDataAction);
        }

        // TODO: Consider using builder
        public TelemetryRouterEventOptions WriteStaticData(string telemetryInfoName, params string[] telemetryWriterUniqueNames)
        {
            var writeDataAction =
                new TelemetryRouterEventWriteDataAction(true, telemetryInfoName, telemetryWriterUniqueNames);
            AddWriteDataAction(writeDataAction);
            return this;
        }

        public TelemetryRouterEventOptions WriteEventData(string telemetryInfoName, params string[] telemetryWriterUniqueNames)
        {
            var writeDataAction =
                new TelemetryRouterEventWriteDataAction(false, telemetryInfoName, telemetryWriterUniqueNames);
            AddWriteDataAction(writeDataAction);
            return this;
        }
    }

    public static class DefaultTelemetryEventNames
    {
        public static string Initialization => "TelemetryRouter.Initialization";
    }

    public class TelemetryRouterEventWriteDataAction
    {
        public TelemetryRouterEventWriteDataAction(
            bool isStatic,
            string telemetryInfoName,
            params string[] telemetryWriterUniqueNames)
        {
            IsStatic = isStatic;
            TelemetryInfoName = telemetryInfoName;
            TelemetryWriterUniqueNames = telemetryWriterUniqueNames;
        }

        public bool IsStatic { get; }

        public string TelemetryInfoName { get; }

        public string[] TelemetryWriterUniqueNames { get; }
    }

    // TODO Remove Static
    public class TelemetryRouter
    {
        private static readonly Dictionary<string, ITelemetryWriter> TelemetryWritersByUniqueName = new();
        private static readonly Dictionary<string, TelemetryRouterEventOptions> EventOptionsByName = new();

        private static readonly TelemetryInfoStorage StaticTelemetryInfoStorage = new();

        internal static void Initialize(
            TelemetryRouterOptions telemetryRouterOptions,
            IServiceProvider serviceProvider)
        {
            foreach (var telemetryWriterType in telemetryRouterOptions.TelemetryWriterTypes)
            {
                var telemetryWriter = (ITelemetryWriter)serviceProvider.GetRequiredService(telemetryWriterType);
                TelemetryWritersByUniqueName.Add(telemetryWriter.WriterUniqueName, telemetryWriter);
            }

            foreach (var telemetryRouterEventOptions in telemetryRouterOptions.EventOptions)
            {
                EventOptionsByName.Add(telemetryRouterEventOptions.EventName, telemetryRouterEventOptions);
            }

            foreach (var telemetryInfoItemCollection in telemetryRouterOptions.StaticTelemetryDataProviders
                         .SelectMany(i => i.GetTelemetryData()))
            {
                StaticTelemetryInfoStorage.AddData(telemetryInfoItemCollection);
            }
        }

        public static void ProcessTelemetryEvent(TelemetryEvent telemetryEvent)
        {
            if (EventOptionsByName.TryGetValue(telemetryEvent.EventName, out var telemetryRouterEventOptions) is false)
                return;

            foreach (var writeDataAction in telemetryRouterEventOptions.EnumerationWriteDataActions())
            {
                ProcessWriteDataAction(writeDataAction, telemetryEvent.TelemetryInfos);
            }
        }

        private static ITelemetryWriter? TryGetTelemetryWriter(string writerUniqueName)
        {
            if (TelemetryWritersByUniqueName.TryGetValue(writerUniqueName, out var telemetryWriter))
                return telemetryWriter;

            return null;
        }

        private static void ProcessWriteDataAction(TelemetryRouterEventWriteDataAction writeDataAction, TelemetryInfo[] telemetryInfos)
        {
            var telemetryWriters = writeDataAction.TelemetryWriterUniqueNames
                .Select(TryGetTelemetryWriter)
                .Where(i => i is not null)
                .Cast<ITelemetryWriter>();

            var telemetryInfosToWrite = writeDataAction.IsStatic
                ? StaticTelemetryInfoStorage.GetDataFor(writeDataAction.TelemetryInfoName).ToArray()
                : telemetryInfos.Where(i => i.TelemetryUniqueName == writeDataAction.TelemetryInfoName).ToArray();

            foreach (var telemetryWriter in telemetryWriters)
                telemetryWriter.Write(telemetryInfosToWrite, writeDataAction.IsStatic);
        }
    }

    public class TelemetryEvent
    {
        public TelemetryEvent(string eventName, params TelemetryInfo[] telemetryInfos)
        {
            EventName = eventName;
            TelemetryInfos = telemetryInfos;
        }

        public string EventName { get; }

        public TelemetryInfo[] TelemetryInfos { get; }
    }

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

        public IEnumerable<TelemetryInfo> GetDataFor(string telemetryUniqueName)
        {
            if (_telemetryInfosByUniqueName.TryGetValue(telemetryUniqueName, out var list))
                return list;

            return Enumerable.Empty<TelemetryInfo>();
        }
    }

    public interface IStaticTelemetryDataProvider
    {
        TelemetryInfo[] GetTelemetryData();
    }

    public class BuildConfigurationStaticTelemetryDataProvider : IStaticTelemetryDataProvider
    {
        public TelemetryInfo[] GetTelemetryData()
        {
            const string buildEnvironmentKeyPrefix = "BUILD_";
            const string telemetryKeyPrefix = "build.";

            var telemetryInfoItemCollection = new TelemetryInfo(
                StaticTelemetryUniqueNames.BuildConfiguration,
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

    public static class StaticTelemetryUniqueNames
    {
        public static string BuildConfiguration => "Static.BuildConfiguration";
    }

    public class InitializeTelemetryRouterHostedService : IHostedService
    {
        private readonly IOptions<TelemetryRouterOptions> _options;
        private readonly IServiceProvider _serviceProvider;

        public InitializeTelemetryRouterHostedService(
            IOptions<TelemetryRouterOptions> options,
            IServiceProvider serviceProvider)
        {
            _options = options;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TelemetryRouter.Initialize(_options.Value, _serviceProvider);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class LogPropertyWriter : ITelemetryWriter
    {
        public string WriterUniqueName => TelemetryWriterUniqueNames.LogProperty;

        public void Write(TelemetryInfo[] telemetryInfos, bool isStaticData)
        {
            LogPropertyTelemetryDataAccessor.AddTelemetryInfos(telemetryInfos, isStaticData);
        }
    }

    public static class LogPropertyTelemetryDataAccessor
    {
        private static readonly List<TelemetryInfo> StaticData = new();
        private static readonly AsyncLocal<EventDataHolder> EventDataCurrent = new();

        internal static void AddTelemetryInfos(TelemetryInfo[] telemetryInfos, bool isStaticData)
        {
            if (isStaticData)
                StaticData.AddRange(telemetryInfos);
            else
            {
                EventDataCurrent.Value ??= new EventDataHolder();
                EventDataCurrent.Value.EventData.AddRange(telemetryInfos);
            }
        }

        public static IEnumerable<TelemetryInfo> GetTelemetryData()
        {
            if (EventDataCurrent.Value is null)
                return Enumerable.Empty<TelemetryInfo>();

            return StaticData.Concat(EventDataCurrent.Value.EventData);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class EventDataHolder
        {
            public readonly List<TelemetryInfo> EventData = new();
        }
    }
}