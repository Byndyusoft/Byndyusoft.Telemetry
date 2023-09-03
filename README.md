# Концепция
В этой библиотеке реализована концепция передачи телеметрии определенных данных к определенным "писателям" этих данных. 
Под писателями подразумевается конкретные секции вывода телеметрии, которые впоследствии будут использоваться человеком для анализа этих данных.

## Писатели
Нас, прежде всего, интересуют следующие виды писателей:
- Свойства логов - это поля json-записей в журнале логов, которые описывают контекст текущего события (например, запроса http, или сервиса в целом). Они нужны для фильтрации в ElasticSearch.
- Логи - это строчка записи в журнал логов, описывающая событие, которое сейчас произошло.
- Тэги спана трассы - это поля текущего спана в трассе, которые описывают контекст текущего события (например, запроса http, или сервиса в целом). Они нужны для фильтрации в Jaeger.
- События спана трассы - это выпуск события в текущий спан в трассе. Оно описывает событие, которое сейчас произошло.

Можно использователей писателей как для трас из OpenTelemetry Tracing, так и для OpenTracing. Ограничений в этой концепции нет.

## Данные телеметрии
Один элемент телеметрии содержит 2 поля: ключ и значение. Он описан в классе [TelemetryInfoItem](src/Telemetry/Data/TelemetryInfoItem.cs).

Эти данные объеденияются в группу данных, которая называется [TelemetryInfo](src/Telemetry/Data/TelemetryInfo.cs). 
Группа содержит некое уникальное строковое обозначение, которое впоследствии будет использоваться для роутинга данных писателям.
Также в ней находится описательное поле Message, оно необходимо для писателей логов и событий спана трассы.

### Данные телеметрии события
При возникновении того или иного события (например, http request) возникают данные, которые нужно куда-то отправить. Они отправляются в виде событий телеметрии [TelemetryEvent](src/Telemetry/Data/TelemetryEvent.cs) через метод *ProcessTelemetryEvent* в [ITelemetryRouter](src/Telemetry/ITelemetryRouter.cs).

Событие телеметрии  содержит уникальное имя *EventName*, которое обозначает момент возникновения этого события. Оно используется для роутинга данных.

### Статические данные телеметрии, или Данные сервиса
При старте приложения можно собрать телеметрию о самом сервисе. Например:
- Версия сервиса.
- Окружение, в котором запущен сервис.

Эти данные называются статическими, и их сбор осуществляется с помощью реализации интерфейса [IStaticTelemetryDataProvider](src/Telemetry/Providers/Interface/IStaticTelemetryDataProvider.cs).
Статические данные собираются при инициализации роутера телеметрии. Она происходит перед стартом сервисе в [InitializeTelemetryRouterHostedService](src/Telemetry/HostedServices/InitializeTelemetryRouterHostedService.cs).
Их можно отправить писателям при любом возникновении события.

Существует специальное событие, которое можно вызвать для отправки статических данных сразу после инициализации роутера телеметрии. Оно называется *TelemetryRouter.Initialization* и описано в классе [DefaultTelemetryEventNames](src/Telemetry/Definitions/DefaultTelemetryEventNames.cs).

## Особенности обогащения логов
Т.к. свойства логов обогащаются пассивно (другими словами, нельзя явно что-то обогатить в контексте возникновения события), то данные телеметрии для их обогащения отправлются в класс [LogPropertyTelemetryDataAccessor](src/Telemetry/Logging/LogPropertyTelemetryDataAccessor.cs) через писателя [LogPropertyWriter](src/Telemetry/Writers/LogPropertyWriter.cs). 
Он является статическим, и хранит в себе как статические данные, так и данные события, хранящиеся внутри асинхронного контекста с помощью [AsyncLocal<T>](https://learn.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1).

Пример обогатителя для логгера Serilog описан в классе [TelemetryLogEventEnricher](src/Telemetry.Serilog/TelemetryLogEventEnricher.cs):
```csharp
public class TelemetryLogEventEnricher : ILogEventEnricher
{
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		foreach (var telemetryInfo in LogPropertyTelemetryDataAccessor.GetTelemetryData())
			Enrich(logEvent, propertyFactory, telemetryInfo);
	}

	private void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, TelemetryInfo telemetryInfo)
	{
		foreach (var telemetryInfoItem in telemetryInfo)
		{
			logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(telemetryInfoItem.Key,
				telemetryInfoItem.Value));
		}
	}
}
```

## Конфигурирование роутинга


[![License](https://img.shields.io/badge/License-Apache--2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

# ExampleProject [![Nuget](https://img.shields.io/nuget/v/ExampleProject.svg)](https://www.nuget.org/packages/ExampleProject/)[![Downloads](https://img.shields.io/nuget/dt/ExampleProject.svg)](https://www.nuget.org/packages/ExampleProject/)

Package description

## Installing

```shell
dotnet add package ExampleProject
```

## Usage

Usage description

```csharp
  TODO
```

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
