# SunAuto.Logging

A standard `Microsoft.Extensions.Logging` provider for Sun Auto applications.

## Installation

```shell
dotnet add package SunAuto.Logging
```

## Usage

### Generic Host / ASP.NET Core

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSunAutoLogging(options =>
{
    options.MinimumLevel    = LogLevel.Information;
    options.IncludeScopes   = true;
    options.IncludeTimestamp = true;
});
```

### Configuration via appsettings.json

```json
{
  "Logging": {
    "SunAuto": {
      "MinimumLevel": "Information",
      "IncludeScopes": true,
      "IncludeTimestamp": true,
      "TimestampFormat": "yyyy-MM-ddTHH:mm:ss.fffZ"
    }
  }
}
```

Then register without a configuration delegate:

```csharp
builder.Logging.AddSunAutoLogging();
```

### Custom Sink

Route log output to any destination by supplying a `Sink` delegate:

```csharp
builder.Logging.AddSunAutoLogging(options =>
{
    options.Sink = line => MyExternalSystem.Write(line);
});
```

When `Sink` is `null` (the default), output goes to `Console.Out`.

## Options

| Property          | Type              | Default                        | Description                                |
|-------------------|-------------------|--------------------------------|--------------------------------------------|
| `MinimumLevel`    | `LogLevel`        | `Information`                  | Lowest level that is emitted               |
| `IncludeScopes`   | `bool`            | `true`                         | Append active log scopes to each line      |
| `IncludeTimestamp`| `bool`            | `true`                         | Prepend a UTC timestamp to each line       |
| `TimestampFormat` | `string`          | `"yyyy-MM-ddTHH:mm:ss.fffZ"`   | `DateTime` format string for the timestamp |
| `Sink`            | `Action<string>?` | `null` (→ Console)             | Custom output target                       |

## Log Format

```
[2026-05-19T14:32:01.123Z] info MyApp.Services.RepairService: Order 1042 authorised. => Scope
```
