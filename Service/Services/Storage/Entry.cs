
using Azure;
using Azure.Data.Tables;

namespace SunAuto.Logging.Api.Services.LoggingStorage;

public class Entry : ITableEntity
{
    public string Application { get; set; } = null!;
    public string Environment { get; set; } = null!;
    public string Level { get; set; } = null!;
    public string Body { get; set; } = null!;

    /// <summary>
    /// AKA ApplicationName
    /// </summary>
    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp { get; set; } = DateTime.UtcNow;
    public ETag ETag { get; set; }
}