namespace SunAuto.Splunk.Client;

public class Entry
{
    public Event @event { get; set; } = null!;
    public string sourcetype { get; set; } = null!;
}