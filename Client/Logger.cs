using System.Text;
using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client;

internal sealed class Logger : ILogger
{
    private readonly string _categoryName;
    private readonly LoggerProvider _provider;

    internal Logger(string categoryName, LoggerProvider provider)
    {
        _categoryName = categoryName;
        _provider = provider;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => _provider.ScopeProvider?.Push(state);

    public bool IsEnabled(LogLevel logLevel)
        => logLevel != LogLevel.None && logLevel >= _provider.Options.MinimumLevel;

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
            Timestamp = DateTime.UtcNow
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
