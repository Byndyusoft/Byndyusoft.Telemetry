﻿

// ReSharper disable once CheckNamespace
namespace Serilog.Configuration
{
    using Byndyusoft.Telemetry.Logging.Serilog.Enrichers;

    public static class LoggerEnrichmentConfigurationExtensions
    {
        public static LoggerConfiguration WithPropertyDataAccessor(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<TelemetryPropertyDataAccessorEnricher>();
        }

        public static LoggerConfiguration WithStaticTelemetryItems(
            this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<TelemetryStaticTelemetryItemsEnricher>();
        }
    }
}