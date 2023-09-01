using System;
using System.Collections.Generic;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Options
{
    public class TelemetryRouterOptions
    {
        internal readonly List<TelemetryRouterEventOptions> EventOptions = new();
        internal readonly List<IStaticTelemetryDataProvider> StaticTelemetryDataProviders = new();

        public TelemetryRouterOptions AddStaticTelemetryDataProvider(IStaticTelemetryDataProvider telemetryDataProvider)
        {
            StaticTelemetryDataProviders.Add(telemetryDataProvider);
            return this;
        }

        public TelemetryRouterOptions AddEvent(string eventName, Action<TelemetryRouterEventOptions> configureOptions)
        {
            var telemetryRouterEventOptions = new TelemetryRouterEventOptions(eventName);
            configureOptions.Invoke(telemetryRouterEventOptions);

            EventOptions.Add(telemetryRouterEventOptions);
            return this;
        }
    }
}