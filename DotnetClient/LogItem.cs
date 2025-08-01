using Microsoft.Extensions.Logging;

namespace SunAuto.Splunk.Client;

/// <summary>
/// Log item
/// </summary>
/// <param name="timeStamp">Time stamp</param>
/// <param name="level">Log level</param>
/// <param name="eventId">Event ID</param>
/// <param name="state">State</param>
/// <param name="exception">Exception information to log.</param>
public class LogItem(DateTime timeStamp, LogLevel level, EventId eventId, string? state, string? exception)
{
    public DateTime TimeStamp { get; private set; } = timeStamp;

    public LogLevel LogLevel { get; private set; } = level;

    public EventId EventId { get; private set; } = eventId;

    public string? State { get; private set; } = state;

    public string? Exception { get; private set; } = exception;
}