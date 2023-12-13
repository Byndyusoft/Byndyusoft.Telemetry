namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Logging
{
    public class PropertyDataItem
    {
        public string Name { get; }

        public object? Value { get; }

        public PropertyDataItem(string name, object? value)
        {
            Name = name;
            Value = value;
        }
    }
}