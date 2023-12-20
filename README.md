[![License](https://img.shields.io/badge/License-Apache--2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

# Byndyusoft.AspNetCore.Mvc.Telemetry [![Nuget](https://img.shields.io/nuget/v/Byndyusoft.AspNetCore.Mvc.Telemetry.svg)](https://www.nuget.org/packages/Byndyusoft.AspNetCore.Mvc.Telemetry/)[![Downloads](https://img.shields.io/nuget/dt/Byndyusoft.AspNetCore.Mvc.Telemetry.svg)](https://www.nuget.org/packages/Byndyusoft.AspNetCore.Mvc.Telemetry/)

This package provides *TelemetryItem* type that is used for logging and tracing purposes. It has only two properties:
 - *Name* represents activity tag key or log object property name.
 - *Value* represents the value of telemetry item.

This package also provides static and object telemetry item collector.

## Installing

```shell
dotnet add package Byndyusoft.AspNetCore.Mvc.Telemetry
```

## Usage

### Static telemetry item collector

To use static telemetry item collector you have to register it with all data providers that implement *IStaticTelemetryItemProvider* interface.

There are four standard providers that are presented in example below.

```csharp
builder.Services.AddStaticTelemetryItemCollector()
    .WithBuildConfiguration()
    .WithAspNetCoreEnvironment()
    .WithServiceName(serviceName)
    .WithApplicationVersion("0.0.0.1");
```


Build configuration provider collects environment variables with names that have **BUILD_** prefix.
ASP NET Core environment configuration provides **ASPNETCORE_ENVIRONMENT** environment value.
Service name and application version providers provide service name and application version respectively =)

These data are available by using *StaticTelemetryItemsCollector* class.

### Object telemetry item collector

*ObjectTelemetryItemsCollector* class is used for extracting parameter telemetry items:
- Currently if parameter is not object (for example, integer or string value) and its name ends with **id** value then it returns the parameter itself.
- If parameter is object then all its public properties marked with *TelemetryItemAttribute* will be extracted.

This class should be used by different instrumentation tools such as:
- Http action filters.
- RabbitMq consumers.
- Kafka consumers.

# Byndyusoft.AspNetCore.Mvc.Telemetry.Abstraction [![Nuget](https://img.shields.io/nuget/v/Byndyusoft.AspNetCore.Mvc.Telemetry.Abstraction.svg)](https://www.nuget.org/packages/Byndyusoft.AspNetCore.Mvc.Telemetry.Abstraction/)[![Downloads](https://img.shields.io/nuget/dt/Byndyusoft.AspNetCore.Mvc.Telemetry.Abstraction.svg)](https://www.nuget.org/packages/Byndyusoft.AspNetCore.Mvc.Telemetry.Abstraction/)

This class provides *TelemetryItemAttribute* attribute that is used by *ObjectTelemetryItemsCollector* described above.

## Installing

```shell
dotnet add package Byndyusoft.AspNetCore.Mvc.Telemetry.Abstraction
```

## Usage

Mark all public properties with *TelemetryItemAttribute* so they will be extracted by *ObjectTelemetryItemsCollector*.

```csharp
public class TestObject
{
	[TelemetryItem]
	public int ToLogInt { get; set; }

	[TelemetryItem]
	public string? ToLogString { get; set; }

	public int NotToLog { get; set; }
}
```

In this example only **ToLogInt** and **ToLogString** properties will be extracted. **NotToLog** will not be extracted.

# Byndyusoft.AspNetCore.Mvc.Telemetry.OpenTelemetry [![Nuget](https://img.shields.io/nuget/v/Byndyusoft.AspNetCore.Mvc.Telemetry.OpenTelemetry.svg)](https://www.nuget.org/packages/Byndyusoft.AspNetCore.Mvc.Telemetry.OpenTelemetry/)[![Downloads](https://img.shields.io/nuget/dt/Byndyusoft.AspNetCore.Mvc.Telemetry.OpenTelemetry.svg)](https://www.nuget.org/packages/Byndyusoft.AspNetCore.Mvc.Telemetry.OpenTelemetry/)

This package is used for integration with OpenTelemetry.

## Installing

```shell
dotnet add package Byndyusoft.AspNetCore.Mvc.Telemetry.OpenTelemetry
```

## Usage

### Static telemetry items in resource

Static telemetry items can be added to OpenTelemetry resource information by *AddStaticTelemetryItems* extension method.

```csharp
builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(ResourceBuilder
            .CreateDefault()
            .AddService(serviceName)
            .AddStaticTelemetryItems())
        .AddConsoleExporter();
});
```

Static data are collected by *StaticTelemetryItemsCollector* class which is described above. All static data providers must be registered before OpenTelemetry.

### Activity enrichment with tags

You can use *ActivityTagEnricher* to enrich activity with tags from telemetry items. Here is an example:

```csharp
var telemetryItem = new TelemetryItem("method.type", "test");
ActivityTagEnricher.Enrich(telemetryItem);
```

# Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog [![Nuget](https://img.shields.io/nuget/v/Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog.svg)](https://www.nuget.org/packages/Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog/)[![Downloads](https://img.shields.io/nuget/dt/Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog.svg)](https://www.nuget.org/packages/Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog/)

This package is used for integration with Serilog.

## Installing

```shell
dotnet add package Byndyusoft.AspNetCore.Mvc.Telemetry.Serilog
```

## Usage

*LogPropertyDataAccessor* is used for collecting telemetry items that are available in async context.

To enrich all logs with static telemetry items collected by *StaticTelemetryItemsCollector* (described above) use *WithPropertyDataAccessor* extension method.
To enrich all logs with telemetry items collected by *LogPropertyDataAccessor* use *WithPropertyDataAccessor* extension method.

```csharp
builder.Host.UseSerilog((_, loggerConfiguration) =>
{
    loggerConfiguration
        .Enrich.WithPropertyDataAccessor()
        .Enrich.WithStaticTelemetryItems();
    loggerConfiguration.WriteTo.Console(new JsonFormatter());
});
```

Here is an example for sending telemetry items to *LogPropertyDataAccessor* in controller action method:

```csharp
LogPropertyDataAccessor.AddTelemetryItem("method.type", "test");
```

# Contributing

To contribute, you will need to setup your local environment, see [prerequisites](#prerequisites). For the contribution and workflow guide, see [package development lifecycle](#package-development-lifecycle).

## Prerequisites

Make sure you have installed all of the following prerequisites on your development machine:

- Git - [Download & Install Git](https://git-scm.com/downloads). OSX and Linux machines typically have this already installed.
- .NET (.net version) - [Download & Install .NET](https://dotnet.microsoft.com/en-us/download/dotnet/).

## Package development lifecycle

- Implement package logic in `src`
- Add or adapt unit-tests (prefer before and simultaneously with coding) in `tests`
- Add or change the documentation as needed
- Open pull request in the correct branch. Target the project's `master` branch

# Maintainers
[github.maintain@byndyusoft.com](mailto:github.maintain@byndyusoft.com)
