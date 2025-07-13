namespace SunAuto.Splunk.Client;

public interface IEntry
{
    string Application { get; set; }

    Guid Id { get; set; }

    DateTime Timestamp { get; set; }

    string Level { get; set; }

    string? Body { get; set; }

    string Message { get; set; }

    int? EventId { get; set; }

    string? EventName { get; set; }
}
