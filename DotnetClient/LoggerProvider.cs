using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SunAuto.Splunk.Client;

/// <summary>
/// Creates <see cref="ILogger"/> instances that send log entries to Splunk.
/// Registered with the alias <c>SunAuto</c> so configuration under
/// <c>Logging:SunAuto</c> is automatically bound via the options system.
/// </summary>
[ProviderAlias("SunAuto")]
public sealed class LoggerProvider(IStorage storage, IOptionsMonitor<LoggerConfiguration> options) :
    ILoggerProvider
{
    readonly Dictionary<string, ILogger> _loggers = [];

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        if (_loggers.TryGetValue(categoryName, out var existing))
            return existing;

        var logger = new Logger(categoryName, options, storage);
        _loggers.Add(categoryName, logger);
        return logger;
    }

    /// <inheritdoc/>
    public void Dispose() { }
}
