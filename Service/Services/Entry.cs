
//using Azure;
//using Azure.Data.Tables;
//using System.Runtime.Serialization;

//namespace SunAuto.Logging.Api.Services;

//public class Entry : Common.Entry, ITableEntity
//{
//    public Entry() =>Timestamp = DateTime.UtcNow;

//    [IgnoreDataMember]
//    public new string Application
//    {
//        get => PartitionKey;
//        set => PartitionKey = value;
//    }

//    /// <summary>
//    /// AKA ApplicationName
//    /// </summary>
//    public string PartitionKey { get; set; } = null!;

//    public ETag ETag { get; set; }
//}