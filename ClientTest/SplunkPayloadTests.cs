using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client.Test;

/// <summary>
/// Covers the bytes the sink puts on the wire. Splunk HEC accepts concatenated JSON objects at
/// <c>services/collector/event</c> and rejects a JSON array, so payload shape is a correctness
/// concern rather than a formatting preference.
/// </summary>
public class SplunkPayloadTests
{
    private static LoggerProvider CreateProvider(FakeHec hec, Action<LoggerOptions>? configure = null)
    {
        var options = new LoggerOptions
        {
            MinimumLevel = LogLevel.Trace,
            Splunk = new LoggerOptions.SplunkOptions
            {
                BaseUrl = hec.BaseUrl,
                Token = "test-token",
                Source = "test-sourcetype",
            },
        };

        configure?.Invoke(options);

        return new LoggerProvider(new TestOptionsMonitor(options));
    }

    [Fact]
    public void Payload_is_not_a_json_array()
    {
        using var hec = new FakeHec();

        using (var provider = CreateProvider(hec))
        {
            var log = provider.CreateLogger("Test");
            log.LogInformation("one");
            log.LogInformation("two");
        }

        Assert.NotEmpty(hec.Bodies);
        Assert.All(hec.Bodies, body => Assert.False(
            body.TrimStart().StartsWith('['),
            $"HEC rejects a JSON array at services/collector/event. Body was: {body}"));
    }

    [Fact]
    public void Every_posted_line_is_a_standalone_json_object()
    {
        using var hec = new FakeHec();

        using (var provider = CreateProvider(hec))
        {
            var log = provider.CreateLogger("Test");
            for (var i = 0; i < 5; i++) log.LogInformation("event {I}", i);
        }

        var events = hec.ParsedEvents();
        Assert.Equal(5, events.Count);
        Assert.All(events, e => Assert.Equal(JsonValueKind.Object, e.ValueKind));
    }

    [Fact]
    public void Envelope_uses_the_lowercase_field_names_hec_requires()
    {
        using var hec = new FakeHec();

        using (var provider = CreateProvider(hec))
            provider.CreateLogger("Test").LogInformation("hello");

        var envelope = Assert.Single(hec.ParsedEvents());

        Assert.True(envelope.TryGetProperty("event", out _), "HEC requires a lowercase 'event' field.");
        Assert.True(envelope.TryGetProperty("sourcetype", out var sourcetype), "HEC requires a lowercase 'sourcetype' field.");
        Assert.Equal("test-sourcetype", sourcetype.GetString());
    }

    [Fact]
    public void Event_carries_level_message_and_timestamp()
    {
        using var hec = new FakeHec();

        using (var provider = CreateProvider(hec))
            provider.CreateLogger("Test").LogWarning("disk nearly full");

        var payload = Assert.Single(hec.ParsedEvents()).GetProperty("event");

        Assert.Equal("Warning", payload.GetProperty("Level").GetString());
        Assert.Equal("disk nearly full", payload.GetProperty("Message").GetString());
        Assert.NotEqual(default, payload.GetProperty("Timestamp").GetDateTime());
    }

    [Fact]
    public void Event_id_and_name_are_forwarded()
    {
        using var hec = new FakeHec();

        using (var provider = CreateProvider(hec))
            provider.CreateLogger("Test").Log(LogLevel.Error, new EventId(42, "PaymentFailed"), "boom");

        var payload = Assert.Single(hec.ParsedEvents()).GetProperty("event");

        Assert.Equal(42, payload.GetProperty("EventId").GetInt32());
        Assert.Equal("PaymentFailed", payload.GetProperty("EventName").GetString());
    }

    [Fact]
    public void Posts_to_the_hec_event_endpoint_with_the_splunk_auth_scheme()
    {
        using var hec = new FakeHec();

        using (var provider = CreateProvider(hec))
            provider.CreateLogger("Test").LogInformation("hello");

        Assert.Equal("/services/collector/event", Assert.Single(hec.Paths));
        Assert.Equal("Splunk test-token", Assert.Single(hec.Authorization));
    }

    [Fact]
    public void Multi_event_batches_stay_newline_delimited()
    {
        using var hec = new FakeHec();

        using (var provider = CreateProvider(hec))
        {
            var log = provider.CreateLogger("Test");
            for (var i = 0; i < 200; i++) log.LogInformation("event {I}", i);
        }

        // At least one request should have carried more than a single event; whichever ones did
        // must still be newline-delimited objects rather than an array.
        Assert.Equal(200, hec.ParsedEvents().Count);
    }

    [Fact]
    public void Sink_delegate_takes_precedence_over_splunk()
    {
        using var hec = new FakeHec();
        var captured = new List<QueueEntry>();

        using (var provider = CreateProvider(hec, o => o.Sink = captured.Add))
            provider.CreateLogger("Test").LogInformation("routed to the delegate");

        var entry = Assert.Single(captured);
        Assert.Equal("routed to the delegate", entry.Formatted);
        Assert.Equal(0, hec.RequestCount);
    }

    [Fact]
    public void Splunk_sink_is_skipped_when_the_token_is_missing()
    {
        using var hec = new FakeHec();

        using (var provider = CreateProvider(hec, o => o.Splunk!.Token = null))
            provider.CreateLogger("Test").LogInformation("no token configured");

        Assert.Equal(0, hec.RequestCount);
    }

    [Fact]
    public void Entries_below_the_minimum_level_are_not_sent()
    {
        using var hec = new FakeHec();

        using (var provider = CreateProvider(hec, o => o.MinimumLevel = LogLevel.Warning))
        {
            var log = provider.CreateLogger("Test");
            log.LogDebug("suppressed");
            log.LogInformation("suppressed");
            log.LogWarning("delivered");
        }

        var payload = Assert.Single(hec.ParsedEvents()).GetProperty("event");
        Assert.Equal("delivered", payload.GetProperty("Message").GetString());
    }

    [Fact]
    public void A_failing_hec_response_does_not_throw_into_the_caller()
    {
        using var hec = new FakeHec(System.Net.HttpStatusCode.BadRequest);

        var exception = Record.Exception(() =>
        {
            using var provider = CreateProvider(hec);
            provider.CreateLogger("Test").LogInformation("server will reject this");
        });

        Assert.Null(exception);
        Assert.Equal(1, hec.RequestCount);
    }
}
