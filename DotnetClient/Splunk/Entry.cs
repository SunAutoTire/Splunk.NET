using SunAuto.Logging.Common;

namespace SunAuto.Logging.Client.Splunk;

public class Entry
{
    public string SourceType { get; set; } = null!;
    public Event Event { get; set; } = null!;
}