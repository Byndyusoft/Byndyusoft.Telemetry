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

var builder = WebApplication
    .CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.Enrich.With<TelemetryLogEventEnricher>()
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
    o.AddRouting(
        TelemetryHttpProviderUniqueNames.Request, 
        TelemetryActivityWriterUniqueNames.Event);
    o.AddRouting(
        TelemetryProviderUniqueNames.BuildConfiguration,
        SerilogTelemetryWriterUniqueNames.Property);
    o.AddWriter<LogWriter>();
    o.AddWriter<ActivityTagWriter>();
    o.AddWriter<ActivityEventWriter>();
    o.AddStaticTelemetryDataProvider(new BuildConfigurationStaticTelemetryDataProvider());
});
builder.Services.AddSingleton<LogWriter>();
builder.Services.AddSingleton<ActivityTagWriter>();
builder.Services.AddSingleton<ActivityEventWriter>();
builder.Services.AddSingleton<ITelemetryRouter, TelemetryRouter>();
builder.Services.AddHostedService<InitializeStaticTelemetryDataHostedService>();

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
