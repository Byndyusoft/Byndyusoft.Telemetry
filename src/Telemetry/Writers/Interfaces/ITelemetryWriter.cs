using Byndyusoft.AspNetCore.Mvc.Telemetry.Data;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Writers.Interfaces
{
    public interface ITelemetryWriter
    {
        string WriterUniqueName { get; }

        void Write(TelemetryInfo[] telemetryInfos, bool isStaticData);
    }
}