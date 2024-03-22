namespace Byndyusoft.Telemetry.Providers.Base
{
    using System;
    using Abstraction;
    using Abstraction.Interfaces;

    public class EnvironmentStaticTelemetryItemProvider : IStaticTelemetryItemProvider
    {
        private readonly string _environmentName;
        private readonly string _telemetryItemName;

        protected EnvironmentStaticTelemetryItemProvider(string environmentName, string telemetryItemName)
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