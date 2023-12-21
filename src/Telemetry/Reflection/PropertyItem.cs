namespace Byndyusoft.Telemetry.Reflection
{
    public class PropertyItem
    {
        public PropertyItem(string propertyName, object? value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        public string PropertyName { get; }

        public object? Value { get; }
    }
}