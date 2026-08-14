using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client.Test;

/// <summary>
/// <see cref="QueueEntry.ToString"/> is the console fallback format and is documented in the
/// README, so it is part of the package's observable surface.
/// </summary>
public class QueueEntryTests
{
    private static readonly DateTime Timestamp = new(2026, 5, 19, 14, 32, 1, DateTimeKind.Utc);

    [Fact]
    public void Renders_timestamp_level_and_message()
    {
        var entry = new QueueEntry
        {
            Loglevel = LogLevel.Information,
            Formatted = "Order 1042 authorised.",
            Timestamp = Timestamp,
        };

        var text = entry.ToString();

        Assert.Contains("2026-05-19T14:32:01", text);
        Assert.Contains("Information", text);
        Assert.Contains("Order 1042 authorised.", text);
    }

    [Fact]
    public void Renders_a_non_zero_event_id_in_brackets()
    {
        var entry = new QueueEntry
        {
            Loglevel = LogLevel.Error,
            EventId = new EventId(42, "PaymentFailed"),
            Formatted = "Charge failed.",
            Timestamp = Timestamp,
        };

        Assert.Contains("[42]", entry.ToString());
    }

    [Fact]
    public void Omits_a_zero_event_id()
    {
        var entry = new QueueEntry
        {
            Loglevel = LogLevel.Information,
            Formatted = "no event id",
            Timestamp = Timestamp,
        };

        Assert.DoesNotContain("[0]", entry.ToString());
    }

    [Fact]
    public void Appends_the_exception_when_present()
    {
        var entry = new QueueEntry
        {
            Loglevel = LogLevel.Error,
            Formatted = "Charge failed.",
            Exception = new InvalidOperationException("gateway timeout"),
            Timestamp = Timestamp,
        };

        var text = entry.ToString();

        Assert.Contains("Exception:", text);
        Assert.Contains("gateway timeout", text);
    }

    [Fact]
    public void Omits_the_exception_section_when_absent()
    {
        var entry = new QueueEntry
        {
            Loglevel = LogLevel.Information,
            Formatted = "all good",
            Timestamp = Timestamp,
        };

        Assert.DoesNotContain("Exception:", entry.ToString());
    }
}
