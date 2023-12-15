using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Configuration;
using Serilog.Formatting.Json;

Environment.SetEnvironmentVariable("BUILD_COMMIT_HASH", "asdfdasf");
Environment.SetEnvironmentVariable("BUILD_VERSION", "1.3.4");

var builder = WebApplication
    .CreateBuilder(args);

builder.Host.UseSerilog((_, loggerConfiguration) =>
{
    loggerConfiguration
        .Enrich.WithPropertyDataAccessor()
        .Enrich.WithStaticTelemetryItems();
    loggerConfiguration.WriteTo.Console(new JsonFormatter());
});

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMvcCore();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var serviceName = builder.Configuration.GetValue<string>("Jaeger:ServiceName");

builder.Services.AddStaticTelemetryItemCollector()
    .WithBuildConfiguration()
    .WithAspNetCoreEnvironment()
    .WithServiceName(serviceName)
    .WithApplicationVersion("0.0.0.1");

builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(ResourceBuilder
            .CreateDefault()
            .AddService(serviceName)
            .AddStaticTelemetryItems())
        .AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = context => context.Request.Path.StartsWithSegments("/swagger") == false;
        })
        .AddConsoleExporter()
        .AddOtlpExporter(builder.Configuration.GetSection("Jaeger").Bind);
});

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