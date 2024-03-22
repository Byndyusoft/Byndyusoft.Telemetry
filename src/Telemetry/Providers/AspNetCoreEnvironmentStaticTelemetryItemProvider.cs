namespace Byndyusoft.Telemetry.Providers
{
    using Base;
    using Consts;

    public class AspNetCoreEnvironmentStaticTelemetryItemProvider : EnvironmentStaticTelemetryItemProvider
    {
        public AspNetCoreEnvironmentStaticTelemetryItemProvider()
            : base("ASPNETCORE_ENVIRONMENT", TelemetryItemNames.Environment)
        {
        }
    }
}