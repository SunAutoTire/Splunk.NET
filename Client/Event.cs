namespace SunAuto.Logging.Client;

/// <summary>
/// Represents a single log event sent to Splunk via the HTTP Event Collector (HEC).
/// </summary>
public class Event
{
    /// <summary>
    /// The serialized exception or structured body associated with this event, if any.
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    /// The log level as a string (e.g., "Information", "Error").
    /// </summary>
    public string Level { get; set; } = null!;

    /// <summary>
    /// The formatted log message.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Optional identifier of the user associated with this event.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// UTC timestamp of when this event was captured.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The numeric ID from the <see cref="Microsoft.Extensions.Logging.EventId"/>, if present.
    /// </summary>
    public int? EventId { get; set; }

    /// <summary>
    /// The name from the <see cref="Microsoft.Extensions.Logging.EventId"/>, if present.
    /// </summary>
    public string? EventName { get; set; }
}

