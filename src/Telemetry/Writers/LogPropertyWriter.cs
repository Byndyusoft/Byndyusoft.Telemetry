using Byndyusoft.AspNetCore.Mvc.Telemetry.Data;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Definitions;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Logging;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Writers.Interfaces;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Writers
{
    public class LogPropertyWriter : ITelemetryWriter
    {
        public string WriterUniqueName => TelemetryWriterUniqueNames.LogPropertyAccessor;

        public void Write(TelemetryInfo[] telemetryInfos, bool isStaticData)
        {
            LogPropertyTelemetryDataAccessor.AddTelemetryInfos(telemetryInfos, isStaticData);
        }
    }
}