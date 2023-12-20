[![License](https://img.shields.io/badge/License-Apache--2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

# Byndyusoft.AspNetCore.Mvc.Telemetry [![Nuget](https://img.shields.io/nuget/v/ExampleProject.svg)](https://www.nuget.org/packages/Byndyusoft.AspNetCore.Mvc.Telemetry/)[![Downloads](https://img.shields.io/nuget/dt/Byndyusoft.AspNetCore.Mvc.Telemetry.svg)](https://www.nuget.org/packages/Byndyusoft.AspNetCore.Mvc.Telemetry/)

This package provides *TelemetryItem* type that is used for logging and tracing purposes. It has only two properties:
 - *Name* represents activity tag key or log object property name.
 - *Value* represents the value of telemetry item.

This package also provides static and object telemetry item collector.

## Installing

```shell
dotnet add package ExampleProject
```

## Usage

### Static telemetry item collector

To use static telemetry item collector you have to register data provider that implements *IStaticTelemetryItemProvider* interface.

There are four standard providers that are presented in example below.

```csharp
builder.Services.AddStaticTelemetryItemCollector()
    .WithBuildConfiguration()
    .WithAspNetCoreEnvironment()
    .WithServiceName(serviceName)
    .WithApplicationVersion("0.0.0.1");
```


Build configuration provider collects environment variables which name has "BUILD_" prefix.
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

# ExampleProject.SecondPackage [![Nuget](https://img.shields.io/nuget/v/ExampleProject.SecondPackage.svg)](https://www.nuget.org/packages/ExampleProject.SecondPackage/)[![Downloads](https://img.shields.io/nuget/dt/ExampleProject.SecondPackage.svg)](https://www.nuget.org/packages/ExampleProject.SecondPackage/)

Package description

## Installing

```shell
dotnet add package ExampleProject.SecondPackage
```

## Usage

Usage description

```csharp
  TODO
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
