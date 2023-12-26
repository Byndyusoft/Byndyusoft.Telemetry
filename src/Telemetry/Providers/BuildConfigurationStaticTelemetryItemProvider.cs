using System;
using System.Collections;
using System.Collections.Generic;
using Byndyusoft.Telemetry.Consts;
using Byndyusoft.Telemetry.Providers.Interface;

namespace Byndyusoft.Telemetry.Providers
{
    public class BuildConfigurationStaticTelemetryItemProvider : IStaticTelemetryItemProvider
    {
        public TelemetryItem[] GetTelemetryItems()
        {
            const string buildEnvironmentKeyPrefix = "BUILD_";
            var telemetryItems = new List<TelemetryItem>();

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
                    var name = property.Remove(0, buildEnvironmentKeyPrefix.Length);
                    name = $"{TelemetryItemNames.BuildPrefix}.{name.ToLowerInvariant()}";
                    telemetryItems.Add(new TelemetryItem(name, value));
                }
            }

            return telemetryItems.ToArray();
        }
    }
}