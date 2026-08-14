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
    options.MinimumLevel = LogLevel.Information;
});
```

### Configuration via appsettings.json

```json
{
  "Logging": {
    "SunAuto": {
      "MinimumLevel": "Information",
      "LogLevel": {
        "Default": "Information"
      }
    }
  }
}
```

Then register without a configuration delegate:

```csharp
builder.Logging.AddSunAutoLogging();
```

### Custom Sink

Route log output to any destination by supplying a `Sink` delegate. The delegate receives a
`QueueEntry`, which carries the level, event ID, formatted message, exception and timestamp:

```csharp
builder.Logging.AddSunAutoLogging(options =>
{
    options.Sink = entry => MyExternalSystem.Write(entry.Formatted, entry.Exception);
});
```

`QueueEntry.ToString()` renders the same single-line format used by the console fallback, so
`options.Sink = entry => MyExternalSystem.Write(entry.ToString());` also works.

When `Sink` is `null` (the default) and no Splunk options are configured, output goes to `Console.Out`.

### Splunk HEC

Set all three Splunk options to enable automatic forwarding to a Splunk HTTP Event Collector.
Entries are queued and posted asynchronously on a background task, so logging never blocks the
caller. Entries that arrive while a post is in flight are coalesced into the next request.

```csharp
builder.Logging.AddSunAutoLogging(options =>
{
    options.Splunk = new()
    {
        BaseUrl = "https://splunk-host:8088/",
        Token   = builder.Configuration["Splunk:Token"],
        Source  = "my-api",
    };
});
```

Or via configuration:

```json
{
    "Logging": {
        "SunAuto": {
            "LogLevel": {
                "Default": "Information"
            },
            "Splunk": {
                "BaseUrl": "https://your-splunk-host:8088/",
                "Source": "my-app-sourcetype"
            }
        }
    }
}
```

**Keep the HEC token out of `appsettings.json`.** Supply it through User Secrets in development
and an environment variable or secret store in deployed environments, so it is never committed:

```shell
dotnet user-secrets set "Logging:SunAuto:Splunk:Token" "<your-hec-token>"
```

```shell
export Logging__SunAuto__Splunk__Token="<your-hec-token>"
```

All three fields (`Splunk.BaseUrl`, `Splunk.Token`, `Splunk.Source`) must be present for the Splunk
sink to activate; if any is missing the sink is skipped and output falls back to the console. If a
custom `Sink` delegate is also provided, it takes precedence and the Splunk sink is not created.

`BaseUrl` should include a trailing slash if it contains a path. `https://host:8088/api` resolves
to `https://host:8088/services/collector/event`, dropping the `/api` segment; `https://host:8088/api/`
resolves as expected. A bare host with no path is unaffected.

## Options

| Property          | Type                  | Default                      | Description                                            |
|-------------------|-----------------------|------------------------------|--------------------------------------------------------|
| `MinimumLevel`    | `LogLevel`            | `Information`                | Lowest level that is emitted                           |
| `Sink`            | `Action<QueueEntry>?` | `null` (→ Console)           | Custom output target; takes precedence over Splunk     |
| `Splunk`          | `SplunkOptions?`      | `null`                       | Splunk HEC settings; all three sub-properties required |
| `Splunk.BaseUrl`  | `string?`             | `null`                       | HEC base URL (e.g. `https://splunk-host:8088/`)        |
| `Splunk.Token`    | `string?`             | `null`                       | HEC authentication token                               |
| `Splunk.Source`   | `string?`             | `null`                       | Splunk `sourcetype` assigned to every event            |

`MinimumLevel` is applied in addition to the standard `Logging:SunAuto:LogLevel` filters, so a
message must satisfy both to be emitted.

> **Not yet implemented.** `IncludeScopes`, `IncludeTimestamp` and `TimestampFormat` exist on
> `LoggerOptions` but are not currently applied to any output. Setting them has no effect.

## Log Format

Console output (the fallback when no `Sink` and no Splunk options are configured) is a single line
per entry, produced by `QueueEntry.ToString()`:

```text
2026-05-19T14:32:01.1234567Z Information  Order 1042 authorised.
2026-05-19T14:32:04.7654321Z Error [42] Charge failed. Exception: System.InvalidOperationException: gateway timeout
```

That is round-trip UTC timestamp, level name, event ID in brackets when non-zero, then the
formatted message, followed by an `Exception:` section when an exception is attached.

## Contributing

Pull requests are welcome. For major changes, please open an issue first.

Run the unit tests before submitting:

```shell
dotnet test ../ClientTest/ClientTest.csproj
```

Please make sure to update tests as appropriate.

## Support

If you like this project and think it has helped in any way, consider getting tires or auto service at a Sun Auto Tire & Service location near you:

<a href="https://sun.auto/home" target="_blank"><img src="https://sun.auto/wp-content/themes/sun-auto/images/logo_sunauto.png" alt="Sun Auto Tire & Service" width="150" height="65"/></a>
