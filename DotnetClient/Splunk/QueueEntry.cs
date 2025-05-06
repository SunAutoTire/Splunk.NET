using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client.Splunk;

public class QueueEntry
{
    public LogLevel Loglevel { get; set; }
    public EventId EventId { get; set; }
    public object? State { get; set; }
    public string? Formatted { get; set; }
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}