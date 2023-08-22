using Byndyusoft.AspNetCore.Mvc.Telemetry;
using Byndyusoft.AspNetCore.Mvc.Telemetry.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services
    .AddMvcCore()
    .AddTracing();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO Вынести в отдельный класс
builder.Services.Configure<TelemetryRouterOptions>(o =>
{
    o.AddRouting(TelemetryProviderUniqueNames.HttpRequest, TelemetryWriterUniqueNames.Log);
    o.AddWriter<LogWriter>();
});
builder.Services.AddSingleton<LogWriter>();
builder.Services.AddSingleton<ITelemetryRouter, TelemetryRouter>();

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
