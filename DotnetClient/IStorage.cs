using Microsoft.Extensions.Logging;

namespace SunAuto.Splunk.Client;

/// <summary>
/// File manager for local development file-based logging.
/// </summary>
/// <param name="path">File path of log file.</param>
public interface IStorage : IDisposable
{
    /// <summary>
    /// Add a log item.
    /// </summary>
    /// <typeparam name="TState">State type</typeparam>
    /// <param name="logLevel">Log level</param>
    /// <param name="eventId">Event ID</param>
    /// <param name="state">State value</param>
    /// <param name="exception">Exception to log</param>
    /// <param name="formatter">Formatter for message.</param>
    void Add<TState>(LogLevel logLevel, EventId eventId, TState? state, Exception? exception, Func<TState, Exception?, string> formatter);
}
