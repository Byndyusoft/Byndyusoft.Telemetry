using System;
using System.Collections.Generic;
using System.Linq;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Data;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Definitions;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Options;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Writers.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry
{
    public class TelemetryRouter : ITelemetryRouter
    {
        private readonly Dictionary<string, TelemetryRouterEventOptions> _eventOptionsByName = new();

        private readonly TelemetryInfoStorage _staticTelemetryInfoStorage = new();
        private readonly Dictionary<string, ITelemetryWriter> _telemetryWritersByUniqueName = new();

        public void ProcessTelemetryEvent(TelemetryEvent telemetryEvent)
        {
            if (_eventOptionsByName.TryGetValue(telemetryEvent.EventName, out var telemetryRouterEventOptions) is false)
                return;

            foreach (var writeDataAction in telemetryRouterEventOptions.EnumerationWriteDataActions())
            {
                ProcessWriteDataAction(writeDataAction, telemetryEvent.TelemetryInfos);
            }
        }

        internal void Initialize(
            TelemetryRouterOptions telemetryRouterOptions,
            IServiceProvider serviceProvider)
        {
            var telemetryWriters = serviceProvider.GetServices<ITelemetryWriter>();
            foreach (var telemetryWriter in telemetryWriters)
            {
                _telemetryWritersByUniqueName.Add(telemetryWriter.WriterUniqueName, telemetryWriter);
            }

            foreach (var telemetryRouterEventOptions in telemetryRouterOptions.EventOptions)
            {
                _eventOptionsByName.Add(telemetryRouterEventOptions.EventName, telemetryRouterEventOptions);
            }

            foreach (var telemetryInfo in telemetryRouterOptions.StaticTelemetryDataProviders
                         .SelectMany(i => i.GetTelemetryData()))
            {
                _staticTelemetryInfoStorage.AddData(telemetryInfo);
            }

            var initializationTelemetryEvent = new TelemetryEvent(DefaultTelemetryEventNames.Initialization);
            ProcessTelemetryEvent(initializationTelemetryEvent);
        }

        private void ProcessWriteDataAction(TelemetryRouterEventWriteDataAction writeDataAction,
            TelemetryInfo[] telemetryInfos)
        {
            var telemetryWriters = writeDataAction.TelemetryWriterUniqueNames
                .Select(TryGetTelemetryWriter)
                .Where(telemetryWriter => telemetryWriter is not null)
                .Cast<ITelemetryWriter>();

            var telemetryInfosToWrite = writeDataAction.IsStatic
                ? _staticTelemetryInfoStorage.GetData(writeDataAction.TelemetryUniqueName).ToArray()
                : telemetryInfos.Where(i => i.TelemetryUniqueName.Equals(writeDataAction.TelemetryUniqueName))
                    .ToArray();

            foreach (var telemetryWriter in telemetryWriters)
                telemetryWriter.Write(telemetryInfosToWrite, writeDataAction.IsStatic);
        }

        private ITelemetryWriter? TryGetTelemetryWriter(string writerUniqueName)
        {
            if (_telemetryWritersByUniqueName.TryGetValue(writerUniqueName, out var telemetryWriter))
                return telemetryWriter;

            return null;
        }
    }
}