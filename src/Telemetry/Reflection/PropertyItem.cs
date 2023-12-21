namespace Byndyusoft.Telemetry.Reflection
{
    public class PropertyItem
    {
        public string PropertyName { get; }

        public object? Value { get; }

        public PropertyItem(string propertyName, object? value)
        {
            PropertyName = propertyName;
            Value = value;
        }
    }
}