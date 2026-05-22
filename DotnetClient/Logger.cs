using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SunAuto.Splunk.Client;

/// <summary>
/// An <see cref="ILogger"/> implementation that forwards log entries to <see cref="IStorage"/>
/// for async delivery to Splunk. Log level filtering respects per-category configuration
/// under <c>Logging:SunAuto:LogLevel</c> and reacts to runtime configuration changes.
/// </summary>
internal class Logger(string categoryName, IOptionsMonitor<LoggerConfiguration> options, IStorage storage) : ILogger
{
    LogLevel CurrentLevel => options.CurrentValue.LogLevel
        .GetValueOrDefault(categoryName,
            options.CurrentValue.LogLevel.GetValueOrDefault("Default", LogLevel.Information));

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="logLevel"/> is at or above the
    /// configured level for this logger's category.
    /// </summary>
    /// <param name="logLevel">The level to test.</param>
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && logLevel >= CurrentLevel;

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
            storage.Add(logLevel, eventId, state, exception, formatter);
    }
}
