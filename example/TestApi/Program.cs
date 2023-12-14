using System;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog;
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
builder.Services.AddMvcCore();
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
            options => { options.Filter = context => context.Request.Path.StartsWithSegments("/swagger") == false; })
        .AddConsoleExporter()
        .AddOtlpExporter(builder.Configuration.GetSection("Jaeger").Bind);
});

// TODO Добавить

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