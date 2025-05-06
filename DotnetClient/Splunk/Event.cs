using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunAuto.Logging.Client.Splunk;
public class Event
{    
    public object? Body { get; set; }
    public string Level { get; set; } = null!;
    public string Message { get; set; } = null!;
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public int? EventId { get; set; }
    public string? EventName { get; set; }
}

