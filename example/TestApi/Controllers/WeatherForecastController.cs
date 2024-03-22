namespace Byndyusoft.TestApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Telemetry.Abstraction;
    using Telemetry.Logging;
    using Telemetry.OpenTelemetry;

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            var telemetryItem = new TelemetryItem("method.type", "test");
            LogPropertyDataAccessor.AddTelemetryItem(telemetryItem);
            ActivityTagEnricher.Enrich(telemetryItem);

            _logger.LogInformation("Get Weather");

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                                                              {
                                                                  Date = DateTime.Now.AddDays(index),
                                                                  TemperatureC = Random.Shared.Next(-20, 55),
                                                                  Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                                                              })
                             .ToArray();
        }
    }
}