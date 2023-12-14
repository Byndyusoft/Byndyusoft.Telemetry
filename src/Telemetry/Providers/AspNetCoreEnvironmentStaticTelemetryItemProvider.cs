using Byndyusoft.AspNetCore.Mvc.Telemetry.Consts;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers.Base;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Providers
{
    public class AspNetCoreEnvironmentStaticTelemetryItemProvider : EnvironmentStaticTelemetryItemProvider
    {
        public AspNetCoreEnvironmentStaticTelemetryItemProvider() 
            : base("ASPNETCORE_ENVIRONMENT", TelemetryItemNames.Environment)
        {
        }
    }
}