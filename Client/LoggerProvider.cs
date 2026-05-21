using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SunAuto.Logging;

/// <summary>
/// A Microsoft.Extensions.Logging provider that routes log entries through a
/// configurable <see cref="LoggerOptions.Sink"/> (or <see cref="Console"/> when
/// no sink is configured).
/// </summary>
[ProviderAlias("SunAuto")]
public sealed class LoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, Logger> _loggers = new(StringComparer.Ordinal);
    private readonly IDisposable? _optionsChangeToken;
    private SplunkSink? _splunkSink;
    private bool _disposed;

    internal LoggerOptions Options { get; private set; }
    internal IExternalScopeProvider? ScopeProvider { get; private set; }

    public LoggerProvider(IOptionsMonitor<LoggerOptions> options)
    {
        Options = options.CurrentValue;
        ApplySplunkSink(Options);
        _optionsChangeToken = options.OnChange(updated =>
        {
            Options = updated;
            ApplySplunkSink(updated);
        });
    }

    private void ApplySplunkSink(LoggerOptions opts)
    {
        if (opts.Sink is not null ||
            opts.SplunkBaseUrl is null ||
            opts.SplunkToken is null ||
            opts.SplunkSource is null)
            return;

        _splunkSink?.Dispose();
        _splunkSink = new SplunkSink(opts.SplunkBaseUrl, opts.SplunkToken, opts.SplunkSource);
        opts.Sink = _splunkSink.Write;
    }

    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new Logger(name, this));

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => ScopeProvider = scopeProvider;

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _optionsChangeToken?.Dispose();
        _splunkSink?.Dispose();
        _loggers.Clear();
    }
}
