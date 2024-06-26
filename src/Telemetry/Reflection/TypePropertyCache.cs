﻿namespace Byndyusoft.Telemetry.Reflection
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Abstraction.Attributes;

    public class TypePropertyCache
    {
        private static readonly ConcurrentDictionary<Type, TypePropertyCache> Caches = new();
        private readonly PropertyInfo[] _properties;

        private TypePropertyCache(Type type)
        {
            Type = type;
            _properties = type
                          .GetProperties()
                          .Where(i => i.GetCustomAttribute<TelemetryItemAttribute>() != null && i.GetMethod != null)
                          .ToArray();
        }

        public Type Type { get; }

        public static PropertyItem[] GetPropertyItems(Type type, object value)
        {
            var typePropertyCache = Caches.GetOrAdd(type, t => new TypePropertyCache(t));
            return typePropertyCache.GetPropertyItems(value);
        }

        public PropertyItem[] GetPropertyItems(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var propertyItems = _properties
                                .Select(i => new PropertyItem(i.Name, i.GetValue(value)))
                                .ToArray();

            return propertyItems;
        }
    }
}