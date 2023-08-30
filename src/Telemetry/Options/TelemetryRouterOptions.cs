using System;
using System.Collections.Generic;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Writers.Interfaces;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Options
{
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
}