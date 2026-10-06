# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Source for the **`SunAuto.Logging`** NuGet package: a `Microsoft.Extensions.Logging` provider that
writes to a custom sink delegate, a Splunk HTTP Event Collector (HEC), or the console.
`README.md` is packed into the nupkg as the package readme, so it is user-facing documentation.
Keep it in sync with behaviour changes.

Downstream consumers reference the **published package**, not this source. Changing code here
affects nothing downstream until a new version is published and the consumer's version is bumped.

| Project | Notes |
|---|---|
| `Client/` | The library. Multi-targets `net10.0;net9.0;net8.0;netstandard2.1`. Assembly/namespace `SunAuto.Logging.Client`, but the PackageId is `SunAuto.Logging`. `GeneratePackageOnBuild` is on, so every build also produces a `.nupkg`. |
| `ClientTest/` | xunit v3 tests (net10.0), run under VSTest via `xunit.runner.visualstudio` 4.x. Don't re-add the `xunit` 2.x package: it makes every `[Fact]` ambiguous (CS0433). |
| `Sample/` | Generic Host console app that exercises the provider. It configures everything through `appsettings.json` and user secrets (`UserSecretsId` is set) for the HEC token. |
| `DotnetClient/`, `DotnetClientTest/` | Stale leftovers holding only `bin/obj/.lscache`, with no source. Ignore them. |

`.vscode/tasks.json` is mostly copied from an Azure Functions project (it points at a nonexistent
`Service/` folder). Only `build-sample` is relevant.

## Commands

```bash
dotnet build Splunk.NET.slnx
dotnet test  ClientTest/ClientTest.csproj
dotnet test  ClientTest/ClientTest.csproj --filter "FullyQualifiedName~SplunkSinkLifecycleTests"
dotnet run   --project Sample/Sample.csproj
dotnet pack  Client/Client.csproj -c Release -o ./artifacts
```

## Release

`.github/workflows/publish-nuget.yml` builds, tests, and packs on pushes to `main` and on PRs.
It publishes to **nuget.org** (OIDC trusted publishing, `production` environment) only when a
`v*` tag is pushed or when someone runs a manual dispatch with `publish=true`. The package version
comes from `<Version>` in `Client/Client.csproj` and must be bumped by hand together with
`<releaseNotes>`. The tag does not set it. Day-to-day work happens on `dev`, which is then merged to `main`.

The build job installs the .NET 10 SDK, which builds every target framework and is needed for the
`.slnx` solution and the net10.0 test and sample projects. The publish job only runs
`dotnet nuget push`, so it stays on .NET 8.

## Architecture

The flow is `Logger.Log` → `QueueEntry` → sink, where the sink is resolved **per call** as
`options.Sink ?? provider.SplunkWrite ?? Console.WriteLine(entry)`:

- **Precedence:** a custom `Sink` delegate wins. The Splunk sink is only used when no delegate is
  set and all three of `Splunk.BaseUrl`/`Token`/`Source` are non-null. Otherwise output goes to
  the console via `QueueEntry.ToString()`.
- **`LoggerProvider`** (`[ProviderAlias("SunAuto")]`, so config lives at `Logging:SunAuto`)
  subscribes to `IOptionsMonitor<LoggerOptions>.OnChange` and **disposes and rebuilds** the
  `SplunkSink` on every options reload.
- **`UserIdResolver`** runs on the caller's thread inside `Logger.Log`, because ambient request
  state (for example `HttpContext`) is gone by the time the background pump posts. Anything that
  needs request context must be captured into `QueueEntry` at log time, not in the sink.
- `IncludeScopes`, `IncludeTimestamp` and `TimestampFormat` on `LoggerOptions` are not wired up to
  anything (the README documents this). `Logger.GetLevelLabel` is also unused.

### SplunkSink concurrency (read before touching it)

`SplunkSink` is a hand-rolled single-consumer pump, and its invariants are documented in the code
comments:

- `Write` enqueues under `_lock` and starts `FlushAsync` only if `_flushing` is false.
  `FlushAsync` drains in batches and clears `_flushing` **under the same lock that observes the
  empty queue**. This is deliberate. If you key off `Task.IsCompleted` instead, entries get
  stranded.
- Each batch is posted as **one HEC request containing newline-delimited JSON `Entry` objects** to
  `services/collector/event`, relative to `BaseUrl`. Without a trailing slash, the last path
  segment of `BaseUrl` is dropped.
- Post failures never throw into the app. They go to `Console.Error` with a `[SunAuto.Logging]`
  prefix.
- `Dispose` sets `_disposed`, waits for the in-flight pump, then synchronously posts anything that
  slipped in. Shutdown must drain rather than drop entries, and the lifecycle tests assert this.

### Payload shape

`Entry { event, sourcetype }` → `Event { Body, Level, Message, Id, UserId, Timestamp, EventId, EventName }`.
`Body` is the exception serialized by `ExceptionConverter`, then re-parsed into a `JsonElement`.

`ExceptionConverter` is write-only and defensive by design. Each reflected property is serialized
to a buffer first, and a throwing getter becomes a diagnostic string. This matters because one
malformed exception would fail the `JsonDocument.Parse` and lose the whole batch. Recursion is
bounded by a `[ThreadStatic]` depth counter (`MaxDepth = 32`), because per-property
`JsonSerializer` calls reset STJ's own depth tracking.

### Tests

Tests use the real `LoggerProvider` with `TestOptionsMonitor` and point it at `FakeHec`, an
in-process `HttpListener` on a free loopback port. `FakeHec` records each request body **before**
it responds, so tests can assert on the exact wire payload after `provider.Dispose()` without
polling or delays. Follow this pattern rather than mocking `HttpClient`.
