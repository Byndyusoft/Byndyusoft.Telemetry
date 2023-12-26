using System;
using System.Linq;
using Byndyusoft.Telemetry.Reflection;

namespace Byndyusoft.Telemetry
{
    public class ObjectTelemetryItemsCollector
    {
        public static TelemetryItem[] Collect(string parameterName, object? value, string namePrefix = "")
        {
            if (value == null)
                return Array.Empty<TelemetryItem>();

            var type = value.GetType();
            if (IsNotObject(type))
                return CollectNotObjectTelemetryValues(parameterName, value, namePrefix);

            var propertyItems = TypePropertyCache.GetPropertyItems(type, value);
            return propertyItems.Select(i => new TelemetryItem(namePrefix + i.PropertyName, i.Value)).ToArray();
        }

        private static bool IsNotObject(Type type)
        {
            var underlyingNullableType = Nullable.GetUnderlyingType(type);
            if (underlyingNullableType is not null)
                type = underlyingNullableType;

            return type.IsPrimitive || type == typeof(string);
        }

        private static TelemetryItem[] CollectNotObjectTelemetryValues(
            string parameterName,
            object? value,
            string namePrefix)
        {
            if (parameterName.EndsWith("id", StringComparison.InvariantCultureIgnoreCase))
                return new[] { new TelemetryItem(namePrefix + parameterName, value) };

            return Array.Empty<TelemetryItem>();
        }
    }
}