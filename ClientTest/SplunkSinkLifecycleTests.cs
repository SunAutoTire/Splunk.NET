using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client.Test;

/// <summary>
/// Covers the queue/flush lifecycle: nothing queued may be silently dropped, and shutdown must
/// drain rather than abandon.
/// </summary>
public class SplunkSinkLifecycleTests
{
    private static LoggerProvider CreateProvider(FakeHec hec) =>
        new(new TestOptionsMonitor(new LoggerOptions
        {
            MinimumLevel = LogLevel.Trace,
            Splunk = new LoggerOptions.SplunkOptions
            {
                BaseUrl = hec.BaseUrl,
                Token = "test-token",
                Source = "test-sourcetype",
            },
        }));

    [Fact]
    public void Dispose_drains_entries_still_queued_at_shutdown()
    {
        using var hec = new FakeHec();
        const int count = 50;

        var provider = CreateProvider(hec);
        var log = provider.CreateLogger("Test");

        // No delay before disposing: entries are still queued or in flight.
        for (var i = 0; i < count; i++) log.LogInformation("event {I}", i);

        provider.Dispose();

        Assert.Equal(count, hec.ParsedEvents().Count);
    }

    [Fact]
    public async Task Concurrent_writers_lose_no_entries()
    {
        using var hec = new FakeHec();
        const int writers = 8;
        const int perWriter = 100;

        var provider = CreateProvider(hec);
        var log = provider.CreateLogger("Test");

        var start = new ManualResetEventSlim();
        var tasks = Enumerable.Range(0, writers).Select(w => Task.Run(() =>
        {
            start.Wait();
            for (var i = 0; i < perWriter; i++)
                log.LogInformation("writer {W} event {I}", w, i);
        })).ToArray();

        start.Set();
        await Task.WhenAll(tasks);

        provider.Dispose();

        Assert.Equal(writers * perWriter, hec.ParsedEvents().Count);
    }

    [Fact]
    public void Entries_written_between_flushes_are_still_delivered()
    {
        using var hec = new FakeHec();
        const int rounds = 20;

        var provider = CreateProvider(hec);
        var log = provider.CreateLogger("Test");

        // Spacing the writes lets the flush pump go idle between them, which exercises the
        // stop/restart path rather than a single continuous drain.
        for (var i = 0; i < rounds; i++)
        {
            log.LogInformation("round {I}", i);
            Thread.Sleep(25);
        }

        provider.Dispose();

        Assert.Equal(rounds, hec.ParsedEvents().Count);
    }

    [Fact]
    public void Writing_after_dispose_is_ignored_rather_than_throwing()
    {
        using var hec = new FakeHec();

        var provider = CreateProvider(hec);
        var log = provider.CreateLogger("Test");

        log.LogInformation("before dispose");
        provider.Dispose();

        var exception = Record.Exception(() => log.LogInformation("after dispose"));

        Assert.Null(exception);
        var payload = Assert.Single(hec.ParsedEvents()).GetProperty("event");
        Assert.Equal("before dispose", payload.GetProperty("Message").GetString());
    }

    [Fact]
    public void Dispose_is_idempotent()
    {
        using var hec = new FakeHec();

        var provider = CreateProvider(hec);
        provider.CreateLogger("Test").LogInformation("hello");

        provider.Dispose();

        Assert.Null(Record.Exception(provider.Dispose));
    }

    [Fact]
    public void Loggers_are_cached_per_category()
    {
        using var hec = new FakeHec();
        using var provider = CreateProvider(hec);

        Assert.Same(provider.CreateLogger("A"), provider.CreateLogger("A"));
        Assert.NotSame(provider.CreateLogger("A"), provider.CreateLogger("B"));
    }
}
