using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client;

/// <summary>
/// Represents a logger that sends log messages to a configured sink, such as Splunk, and provides methods for logging messages at various log levels. It implements the ILogger interface from Microsoft.Extensions.Logging.
/// </summary>
internal sealed class Logger : ILogger
{
    private readonly string _categoryName;
    private readonly LoggerProvider _provider;

    /// <summary>
    /// Initializes a new instance of the Logger class with the specified category name and provider. The category name is used to identify the source of log messages, and the provider is responsible for handling the logging operations.
    /// </summary>
    /// <param name="categoryName">The name of the category for the logger.</param>
    /// <param name="provider">The logger provider responsible for handling logging operations.</param>
    internal Logger(string categoryName, LoggerProvider provider)
    {
        _categoryName = categoryName;
        _provider = provider;
    }

    /// <summary>
    /// Begins a logical operation scope. This method is used to create a scope for logging, which can be useful for grouping related log entries together. It returns an IDisposable that, when disposed, will end the scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state object to be logged.</typeparam>
    /// <param name="state">The state object to be logged.</param>
    /// <returns>An IDisposable that ends the logical operation scope when disposed.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _provider.ScopeProvider?.Push(state);

    /// <summary>
    /// Determines whether the specified log level is enabled for this logger. It checks if the log level is not None and if it meets or exceeds the minimum log level defined in the provider's options.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>True if the specified log level is enabled; otherwise, false.</returns>
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && logLevel >= _provider.Options.MinimumLevel;

    /// <summary>
    /// Logs a message with the specified log level, event ID, state, exception, and formatter function. If the log level is enabled, it creates a QueueEntry and sends it to the configured sink or writes it to the console if no sink is configured.     
    /// </summary>
    /// <typeparam name="TState">The type of the state object to be logged.</typeparam>
    /// <param name="logLevel">The log level of the message.</param>
    /// <param name="eventId">The event ID associated with the log message.</param>
    /// <param name="state">The state object to be logged.</param>
    /// <param name="exception">The exception associated with the log message, if any.</param>
    /// <param name="formatter">A function that formats the state and exception into a log message string.</param>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var options = _provider.Options;

        var entry = new QueueEntry
        {
            Loglevel = logLevel,
            EventId = eventId,
            State = state,
            Exception = exception,
            Formatted = formatter(state!, exception),
            Timestamp = DateTime.UtcNow,
            UserId = options.UserIdResolver?.Invoke(),
        };

        var sink = options.Sink ?? _provider.SplunkWrite;

        if (sink is not null)
            sink(entry);
        else
            Console.WriteLine(entry);
    }

    private static string GetLevelLabel(LogLevel level) => level switch
    {
        LogLevel.Trace => "trce",
        LogLevel.Debug => "dbug",
        LogLevel.Information => "info",
        LogLevel.Warning => "warn",
        LogLevel.Error => "fail",
        LogLevel.Critical => "crit",
        _ => "    "
    };
}
