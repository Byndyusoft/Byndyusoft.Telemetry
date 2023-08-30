using System;
using System.Collections;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Data;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Definitions;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Providers
{
    public class BuildConfigurationStaticTelemetryDataProvider : IStaticTelemetryDataProvider
    {
        public TelemetryInfo[] GetTelemetryData()
        {
            const string buildEnvironmentKeyPrefix = "BUILD_";
            const string telemetryKeyPrefix = "build.";

            var telemetryInfo = new TelemetryInfo(
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
                    telemetryInfo.Add(key, value);
                }
            }

            return new[] { telemetryInfo };
        }
    }
}