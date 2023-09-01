using System.Collections.Generic;
using System.Text;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Data;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Definitions;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Writers.Interfaces;
using Microsoft.Extensions.Logging;

namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Writers
{
    public class LogWriter : ITelemetryWriter
    {
        private readonly ILogger<LogWriter> _logger;

        public LogWriter(ILogger<LogWriter> logger)
        {
            _logger = logger;
        }

        public string WriterUniqueName => TelemetryWriterUniqueNames.Log;

        public void Write(TelemetryInfo[] telemetryInfos, bool isStaticData)
        {
            foreach (var telemetryInfo in telemetryInfos)
                Write(telemetryInfo);
        }

        public void Write(TelemetryInfo telemetryInfo)
        {
            var messageBuilder = new StringBuilder($"{telemetryInfo.Message}: ");
            var arguments = new List<object?>();

            foreach (var telemetryInfoItem in telemetryInfo)
            {
                messageBuilder.Append($" {telemetryInfoItem.Key} = {{{telemetryInfoItem.Key}}};");
                arguments.Add(telemetryInfoItem.Value);
            }

            _logger.LogInformation(messageBuilder.ToString(), arguments.ToArray());
        }
    }
}