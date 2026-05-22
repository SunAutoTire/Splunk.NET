# SunAuto.Logging

A standard `Microsoft.Extensions.Logging` provider for Sun Auto applications. Supports console output, a custom sink delegate, and built-in forwarding to a Splunk HTTP Event Collector (HEC).

## Target Frameworks

`net8.0` · `net9.0` · `net10.0` · `netstandard2.1`

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

When `Sink` is `null` (the default) and no Splunk options are configured, output goes to `Console.Out`.

### Splunk HEC

Set all three Splunk options to enable automatic forwarding to a Splunk HTTP Event Collector. Log entries are batched and posted asynchronously.

```csharp
builder.Logging.AddSunAutoLogging(options =>
{
    options.Splunk = new()
    {
        BaseUrl = "https://splunk-host:8088/",
        Token   = "<your-hec-token>",
        Source  = "my-api",
    };
});
```

Or via `appsettings.json`:

```json
{
    "Logging": {
        "SunAuto": {
            "LogLevel": {
                "Default": "Information"
            },
            "Splunk": {
                "BaseUrl": "https://your-splunk-host:8088/",
                "Token": "your-hec-token",
                "Source": "my-app-sourcetype"
            }
        }
}
```

All three fields (`Splunk.BaseUrl`, `Splunk.Token`, `Splunk.Source`) must be present for the Splunk sink to activate. If a custom `Sink` delegate is also provided, it takes precedence and the Splunk sink is not created.

## Options

| Property          | Type              | Default                        | Description                                            |
|-------------------|-------------------|--------------------------------|--------------------------------------------------------|
| `MinimumLevel`    | `LogLevel`        | `Information`                  | Lowest level that is emitted                           |
| `IncludeScopes`   | `bool`            | `true`                         | Append active log scopes to each line                  |
| `IncludeTimestamp`| `bool`            | `true`                         | Prepend a UTC timestamp to each line                   |
| `TimestampFormat` | `string`          | `"yyyy-MM-ddTHH:mm:ss.fffZ"`   | `DateTime` format string for the timestamp             |
| `Sink`            | `Action<string>?` | `null` (→ Console)             | Custom output target; takes precedence over Splunk     |
| `Splunk`          | `SplunkOptions?`  | `null`                         | Splunk HEC settings; all three sub-properties required |
| `Splunk.BaseUrl`  | `string?`         | `null`                         | HEC base URL (e.g. `https://splunk-host:8088/`)        |
| `Splunk.Token`    | `string?`         | `null`                         | HEC authentication token                               |
| `Splunk.Source`   | `string?`         | `null`                         | Splunk `sourcetype` assigned to every event            |

## Log Format

```text
[2026-05-19T14:32:01.123Z] info MyApp.Services.RepairService: Order 1042 authorised. => Scope
```

Level labels: `trce` · `dbug` · `info` · `warn` · `fail` · `crit`

## Contributing

Pull requests are welcome. For major changes, please open an issue first.

Run the unit tests before submitting:

```shell
dotnet test ../ClientTest/ClientTest.csproj
```

Please make sure to update tests as appropriate.
