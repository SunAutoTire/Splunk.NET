using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace SunAuto.Logging.Client.FileStorage;

/// <summary>
/// File manager for local development file-based logging.
/// </summary>
/// <param name="path">File path of log file.</param>
public class Storage(string path) : IStorage
{
    private readonly string Path = path;

    /// <summary>
    /// Add a log item.
    /// </summary>
    /// <typeparam name="TState">State type</typeparam>
    /// <param name="logLevel">Log level</param>
    /// <param name="eventId">Event ID</param>
    /// <param name="state">State value</param>
    /// <param name="exception">Exception to log</param>
    /// <param name="formatter">Formatter for message.</param>
    public void Add<TState>(LogLevel logLevel, EventId eventId, TState? state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!string.IsNullOrWhiteSpace(Path))
        {
            var formatted = formatter(state!, exception).Quote();
            var datetime = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
            var eventformatted = $"{eventId.Id}:{eventId.Name}";
            var line = $"{datetime},{logLevel},{eventformatted},{formatted},{exception.Quote()}";

            using var writer = new StreamWriter(Path, true);
            writer.WriteLine(line);
            writer.Flush();
            writer.Close();
        }
    }

    // /// <summary>
    // /// Delete item
    // /// </summary>
    // /// <param name="eventId">Event to delete.</param>
    // /// <remarks>Not supported in production environments</remarks>
    // public void Delete(EventId eventId) => throw new NotImplementedException();

    // /// <summary>
    // /// List all items for review by GUI.
    // /// </summary>
    // /// <returns>Collection of items.</returns>
    // public IEnumerable<LogItem> List()
    // {
    //     var records = new List<LogItem>();
    //     var formatprovider = new DateTimeParseFormatProvider();
    //     using var reader = new StreamReader(Path);
    //     using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

    //     csv.Context.RegisterClassMap<LogItemMap>();
    //     csv.Read();

    //     while (csv.Read())
    //     {
    //         var record = new LogItem(DateTime.Parse(csv.GetField(0)!, formatprovider), csv.GetField(1).ToLogLevel(), csv.GetField(2)!.ToEventId(), csv.GetField(3), csv.GetField(4));
    //         records.Add(record);
    //     }

    //     return [.. records];
    // }

    /// <summary>
    /// Mapping for CSV-based storage.
    /// </summary>
    /// <remarks>From CSV Helper Nuget package. <seealso cref="https://joshclose.github.io/CsvHelper/getting-started/"/>Csv Helper (Getting Started)</remarks>
    class LogItemMap : ClassMap<LogItem>
    {
        public LogItemMap()
        {
            Map(m => m.EventId).Index(2);
            Map(m => m.Exception).Index(4);
            Map(m => m.LogLevel).Index(1);
            Map(m => m.State).Index(3);
            Map(m => m.TimeStamp).Index(0);
        }
    }

    /// <summary>
    /// Format provider.
    /// </summary>
    /// <remarks>Can be dropped by just using a more serializable value format in the storage file.</remarks>
    class DateTimeParseFormatProvider : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// Convert the date time string value into a parsable string.
        /// </summary>
        /// <param name="format">Not used</param>
        /// <param name="arg">Value to format</param>
        /// <param name="formatProvider">Format provider instance of <code>DateTimeParseFormatProvider</code></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            ArgumentNullException.ThrowIfNull(arg);

            if (arg.GetType() != typeof(string))
                throw new ArgumentException("Object to format must be of string type.", nameof(arg));

            var value = arg.ToString()!;

            var year = Int32.Parse(value[..4]);
            var month = Int32.Parse(value[6..2]); ;
            var date = Int32.Parse(value[9..2]); ;
            var hour = Int32.Parse(value[12..2]); ;
            var minute = Int32.Parse(value[15..2]); ;
            var second = Int32.Parse(value[18..2]); ;

            return new DateTime(year, month, date, hour, minute, second).ToString();
        }

        /// <summary>
        /// Check format type
        /// </summary>
        /// <param name="formatType">Format type to check.</param>
        /// <returns>True if input format type is <code>DateTimeFormatInfo</code>.</returns>
        public object? GetFormat(Type? formatType) => formatType == typeof(DateTimeFormatInfo) ? this : null;
    }

    bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Storage()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
