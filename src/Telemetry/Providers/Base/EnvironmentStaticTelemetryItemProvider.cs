﻿using System;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Base
{
    public class EnvironmentStaticTelemetryItemProvider : IStaticTelemetryItemProvider
    {
        private readonly string _environmentName;
        private readonly string _telemetryItemName;

        public EnvironmentStaticTelemetryItemProvider(string environmentName, string telemetryItemName)
        {
            _environmentName = environmentName;
            _telemetryItemName = telemetryItemName;
        }

        public TelemetryItem[] GetTelemetryItems()
        {
            return new[]
            {
                new TelemetryItem(_telemetryItemName, Environment.GetEnvironmentVariable(_environmentName))
            };
        }
    }
}