namespace SunAuto.Logging.Common;

public class Entry 
{
    public string Application { get; set; } = null!;

    public string? Message { get; set; }
    public string Level { get; set; } = null!;
    public object? Body { get; set; } 

    public DateTimeOffset? Timestamp { get; set; }
}