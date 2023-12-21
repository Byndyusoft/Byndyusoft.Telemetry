namespace Byndyusoft.Telemetry
{
    public class TelemetryItem
    {
        public string Name { get; }

        public object? Value { get; }

        public TelemetryItem(string name, object? value)
        {
            Name = name;
            Value = value;
        }
    }
}