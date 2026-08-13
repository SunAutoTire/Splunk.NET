using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SunAuto.Logging.Client;

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

    internal Action<QueueEntry>? SplunkWrite => _splunkSink is null ? null : _splunkSink.Write;

    private void ApplySplunkSink(LoggerOptions opts)
    {
        _splunkSink?.Dispose();
        _splunkSink = null;

        if (opts.Splunk?.BaseUrl is null ||
            opts.Splunk.Token is null ||
            opts.Splunk.Source is null)
            return;

        _splunkSink = new SplunkSink(opts.Splunk.BaseUrl, opts.Splunk.Token, opts.Splunk.Source);
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
