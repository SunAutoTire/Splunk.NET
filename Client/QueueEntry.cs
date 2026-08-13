using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client;

public class QueueEntry
{
    public LogLevel Loglevel { get; set; }
    public EventId EventId { get; set; }
    public object? State { get; set; }
    public string? Formatted { get; set; }
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    override public string ToString()
    {
        var levelLabel = Logger.GetLevelLabel(Loglevel);
        var eventId = EventId.Id != 0 ? $"[{EventId.Id}]" : string.Empty;
        var exceptionMessage = Exception is not null ? $" Exception: {Exception}" : string.Empty;
        return $"{Timestamp:O} {levelLabel} {eventId} {Formatted}{exceptionMessage}";
    }
}