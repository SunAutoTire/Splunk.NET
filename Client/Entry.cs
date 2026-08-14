using System.Text.Json.Serialization;

namespace SunAuto.Logging.Client;

/// <summary>
/// Represents a single log entry that will be sent to Splunk via the HTTP Event Collector (HEC).
/// </summary>
public class Entry
{
    /// <summary>
    /// The log event to be sent to Splunk via the HTTP Event Collector (HEC).
    /// </summary>
    [JsonPropertyName("event")]
    public Event @Event { get; set; } = null!;

    /// <summary>
    /// The Splunk 'sub-index' where this event should be stored.
    /// </summary>
    [JsonPropertyName("sourcetype")]
    public string SourceType { get; set; } = null!;
}