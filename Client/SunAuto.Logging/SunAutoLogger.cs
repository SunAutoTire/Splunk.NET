using System.Text;
using Microsoft.Extensions.Logging;

namespace SunAuto.Logging;

internal sealed class SunAutoLogger : ILogger
{
    private readonly string _categoryName;
    private readonly SunAutoLoggerProvider _provider;

    internal SunAutoLogger(string categoryName, SunAutoLoggerProvider provider)
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
        var sb = new StringBuilder();

        if (options.IncludeTimestamp)
        {
            sb.Append('[');
            sb.Append(DateTime.UtcNow.ToString(options.TimestampFormat));
            sb.Append("] ");
        }

        sb.Append(GetLevelLabel(logLevel));
        sb.Append(' ');
        sb.Append(_categoryName);

        if (eventId.Id != 0 || eventId.Name is not null)
        {
            sb.Append('[');
            if (eventId.Name is not null)
                sb.Append(eventId.Name);
            else
                sb.Append(eventId.Id);
            sb.Append(']');
        }

        sb.Append(": ");
        sb.Append(formatter(state, exception));

        if (options.IncludeScopes && _provider.ScopeProvider is not null)
        {
            _provider.ScopeProvider.ForEachScope(
                (scope, builder) =>
                {
                    builder.Append(" => ");
                    builder.Append(scope);
                },
                sb);
        }

        if (exception is not null)
        {
            sb.AppendLine();
            sb.Append(exception);
        }

        var line = sb.ToString();

        if (options.Sink is not null)
            options.Sink(line);
        else
            Console.WriteLine(line);
    }

    private static string GetLevelLabel(LogLevel level) => level switch
    {
        LogLevel.Trace       => "trce",
        LogLevel.Debug       => "dbug",
        LogLevel.Information => "info",
        LogLevel.Warning     => "warn",
        LogLevel.Error       => "fail",
        LogLevel.Critical    => "crit",
        _                    => "    "
    };
}
