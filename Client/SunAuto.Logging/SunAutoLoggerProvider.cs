using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SunAuto.Logging;

/// <summary>
/// A Microsoft.Extensions.Logging provider that routes log entries through a
/// configurable <see cref="SunAutoLoggerOptions.Sink"/> (or <see cref="Console"/> when
/// no sink is configured).
/// </summary>
[ProviderAlias("SunAuto")]
public sealed class SunAutoLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, SunAutoLogger> _loggers = new(StringComparer.Ordinal);
    private readonly IDisposable? _optionsChangeToken;
    private bool _disposed;

    internal SunAutoLoggerOptions Options { get; private set; }
    internal IExternalScopeProvider? ScopeProvider { get; private set; }

    public SunAutoLoggerProvider(IOptionsMonitor<SunAutoLoggerOptions> options)
    {
        Options = options.CurrentValue;
        _optionsChangeToken = options.OnChange(updated => Options = updated);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new SunAutoLogger(name, this));
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        ScopeProvider = scopeProvider;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _optionsChangeToken?.Dispose();
        _loggers.Clear();
    }
}
