using System;
using Byndyusoft.AspNetCore.Mvc.Telemetry;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Http;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
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

var otlpExporterOptions = new OtlpExporterOptions();
builder.Configuration.GetSection("Jaeger").Bind(otlpExporterOptions);
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
builder.Services.Configure<TelemetryRouterOptions>(o =>
{
    o.AddEvent(TelemetryHttpEventNames.Request, eventOptions => eventOptions
        .WriteEventData(
            TelemetryHttpProviderUniqueNames.Request, 
            TelemetryActivityWriterUniqueNames.Event,
            TelemetryActivityWriterUniqueNames.Tag,
            TelemetryWriterUniqueNames.LogProperty)
        .WriteStaticData(
            StaticTelemetryProviderUniqueNames.BuildConfiguration,
            TelemetryActivityWriterUniqueNames.Tag));
    o.AddEvent(DefaultTelemetryEventNames.Initialization, eventOptions => eventOptions
        .WriteStaticData(
            StaticTelemetryProviderUniqueNames.BuildConfiguration, 
            TelemetryWriterUniqueNames.LogProperty));
    o.AddWriter<LogWriter>();
    o.AddWriter<LogPropertyWriter>();
    o.AddWriter<ActivityTagWriter>();
    o.AddWriter<ActivityEventWriter>();
    o.AddStaticTelemetryDataProvider(new BuildConfigurationStaticTelemetryDataProvider());
});
builder.Services.AddSingleton<LogWriter>();
builder.Services.AddSingleton<LogPropertyWriter>();
builder.Services.AddSingleton<ActivityTagWriter>();
builder.Services.AddSingleton<ActivityEventWriter>();
builder.Services.AddHostedService<InitializeTelemetryRouterHostedService>();

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
