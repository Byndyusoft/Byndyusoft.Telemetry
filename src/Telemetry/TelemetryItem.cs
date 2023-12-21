namespace Byndyusoft.Telemetry
{
    public class TelemetryItem
    {
        public TelemetryItem(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object? Value { get; }
    }
}