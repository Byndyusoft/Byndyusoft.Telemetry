using Byndyusoft.Telemetry.Consts;
using Byndyusoft.Telemetry.Providers.Base;

namespace Byndyusoft.Telemetry.Providers
{
    public class AspNetCoreEnvironmentStaticTelemetryItemProvider : EnvironmentStaticTelemetryItemProvider
    {
        public AspNetCoreEnvironmentStaticTelemetryItemProvider() 
            : base("ASPNETCORE_ENVIRONMENT", TelemetryItemNames.Environment)
        {
        }
    }
}