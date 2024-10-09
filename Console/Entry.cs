using Azure;
using Azure.Data.Tables;

namespace SunAuto.Logging.Console;

public class Entry : ITableEntity
{
    //public string Application
    //{
    //    get => PartitionKey;
    //    set => PartitionKey = value;
    //}

    /// <summary>
    /// AKA ApplicationName
    /// </summary>
    public string PartitionKey { get; set; } = null!;

    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; } = DateTime.UtcNow;
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
}
