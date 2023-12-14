namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Interface
{
    public interface IStaticTelemetryItemProvider
    {
        TelemetryItem[] GetTelemetryItems();
    }
}