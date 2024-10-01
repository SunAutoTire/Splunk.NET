namespace SunAuto.Logging.Common;

public class Entry 
{
    public string Application { get; set; } = null!;

    public string? Message { get; set; }
    public string Level { get; set; } = null!;
    public string? Body { get; set; } 

    public DateTimeOffset? Timestamp { get; set; }
    public string RowKey { get; set; } = null!;
}