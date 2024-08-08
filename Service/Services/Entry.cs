
using Azure;
using Azure.Data.Tables;
using System.Runtime.Serialization;

namespace SunAuto.Logging.Api.Services;

public class Entry : Common.Entry, ITableEntity
{
    public Entry() =>Timestamp = DateTime.UtcNow;

    [IgnoreDataMember]
    public new string Application
    {
        get => PartitionKey;
        set => PartitionKey = value;
    }

    //public string? Message { get; set; }
    //public string Level { get; set; } = null!;
    //public string Body { get; set; } = null!;

    /// <summary>
    /// AKA ApplicationName
    /// </summary>
    public string PartitionKey { get; set; } = null!;
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    //public DateTimeOffset? Timestamp { get; set; } = DateTime.UtcNow;
    public ETag ETag { get; set; }
}