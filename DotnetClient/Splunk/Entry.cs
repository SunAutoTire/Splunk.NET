using SunAuto.Logging.Common;

namespace SunAuto.Logging.Client.Splunk;

public class Entry : IEntry
{
    public string Application { get; set; } = null!;
    public string? Body { get; set; }
    public string Level { get; set; } = null!;
    public string Message { get; set; } = null!;
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int? EventId { get; set; }
    public string? EventName { get; set; }
}