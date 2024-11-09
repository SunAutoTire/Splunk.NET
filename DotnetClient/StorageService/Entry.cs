using SunAuto.Logging.Common;

namespace SunAuto.Logging.Client.StorageService;

public class Entry : IEntry
{
    public string Application { get; set; } = null!;
    public string? Body { get; set; }
    public string Level { get; set; } = null!;
    public string Message { get; set; } = null!;
    public Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public DateTime Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int? EventId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string? EventName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}