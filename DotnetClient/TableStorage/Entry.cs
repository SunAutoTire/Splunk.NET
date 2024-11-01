using SunAuto.Logging.Common;

namespace SunAuto.Logging.Client.TableStorage;

public class Entry : IEntry
{
    public string Application { get; set; }
    public string Body { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
    public Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public DateTime Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int? EventId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string? EventName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}