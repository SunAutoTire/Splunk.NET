using Azure;
using Azure.Data.Tables;
using System.Runtime.Serialization;

namespace CleanTableLogs
{
    public class Entry : SunAuto.Logging.Common.Entry, Azure.Data.Tables.ITableEntity
    {
        public Entry() => Timestamp = DateTime.UtcNow;

        [IgnoreDataMember]
        public new string Application
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        /// <summary>
        /// AKA ApplicationName
        /// </summary>
        public string PartitionKey { get; set; } = null!;

        public ETag ETag { get; set; }
    }
}