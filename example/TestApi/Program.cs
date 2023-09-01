using System;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Definitions;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Http;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Providers;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Writers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Json;

Environment.SetEnvironmentVariable("BUILD_COMMIT_HASH", "asdfdasf");
Environment.SetEnvironmentVariable("BUILD_VERSION", "1.3.4");

var builder = WebApplication
    .CreateBuilder(args);

builder.Host.UseSerilog((_, loggerConfiguration) =>
{
    loggerConfiguration.Enrich.With<TelemetryLogEventEnricher>();
    loggerConfiguration.WriteTo.Console(new JsonFormatter());
});

// Add services to the container.

builder.Services.AddControllers();
builder.Services
    .AddMvcCore()
    .AddTracing();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    var serviceName = builder.Configuration.GetValue<string>("Jaeger:ServiceName");
    tracerProviderBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService(serviceName))
        .AddAspNetCoreInstrumentation(
            options =>
            {
                options.Filter = context => context.Request.Path.StartsWithSegments("/swagger") == false;
            })
        .AddConsoleExporter()
        .AddOtlpExporter(builder.Configuration.GetSection("Jaeger").Bind);
});

// TODO Вынести в отдельный класс
builder.Services.AddTelemetryRouter(telemetryRouterOptions => telemetryRouterOptions
    .AddEvent(TelemetryHttpEventNames.Request, eventOptions => eventOptions
        .WriteEventData(HttpTelemetryUniqueNames.Request)
        .To(TelemetryActivityWriterUniqueNames.Event,
            TelemetryActivityWriterUniqueNames.Tag,
            TelemetryWriterUniqueNames.LogPropertyAccessor)
        .WriteStaticData(StaticTelemetryUniqueNames.BuildConfiguration)
        .To(TelemetryActivityWriterUniqueNames.Tag))
    .AddEvent(DefaultTelemetryEventNames.Initialization, eventOptions => eventOptions
        .WriteStaticData(StaticTelemetryUniqueNames.BuildConfiguration)
        .To(TelemetryWriterUniqueNames.LogPropertyAccessor))
    .AddWriter<LogWriter>()
    .AddWriter<LogPropertyWriter>()
    .AddWriter<ActivityTagWriter>()
    .AddWriter<ActivityEventWriter>()
    .AddStaticTelemetryDataProvider(new BuildConfigurationStaticTelemetryDataProvider()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
