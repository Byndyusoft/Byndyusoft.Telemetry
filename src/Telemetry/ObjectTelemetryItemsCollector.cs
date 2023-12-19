using System;
using System.Linq;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Reflection;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry
{
    public class ObjectTelemetryItemsCollector
    {
        public TelemetryItem[] Collect(string parameterName, object? value, string namePrefix = "")
        {
            if (string.IsNullOrEmpty(namePrefix))
                namePrefix = "";
            else if (namePrefix.EndsWith(".") == false)
                namePrefix += '.';

            if (value == null)
                return Array.Empty<TelemetryItem>();

            var type = value.GetType();
            if (IsNotObject(type))
                return CollectNotObjectTelemetryValues(parameterName, value, namePrefix);

            var propertyItems = TypePropertyCache.GetPropertyItems(type, value);
            return propertyItems.Select(i => new TelemetryItem(namePrefix + i.PropertyName, i.Value)).ToArray();
        }

        private bool IsNotObject(Type type)
        {
            return type.IsPrimitive || type == typeof(string);
        }

        private TelemetryItem[] CollectNotObjectTelemetryValues(string parameterName, object? value, string namePrefix)
        {
            if (parameterName.EndsWith("id", StringComparison.InvariantCultureIgnoreCase))
                return new[] { new TelemetryItem(namePrefix + parameterName, value) };

            return Array.Empty<TelemetryItem>();
        }
    }
}