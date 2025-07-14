namespace SunAuto.Splunk.Client.Splunk;

public class Entry
{
    public Event @event { get; set; } = null!;
    public string sourcetype { get; set; } = null!;
}