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

    /// <summary>
    /// Gets the current <see cref="LoggerOptions"/> for this provider. This property reflects the latest configuration values and is updated whenever the options change.
    /// </summary>
    internal LoggerOptions Options { get; private set; }

    /// <summary>
    /// Gets the external scope provider used by this logger provider. This property allows access to the scope provider, which is responsible for managing logging scopes and their associated state. It can be null if no external scope provider has been set.
    /// </summary>
    internal IExternalScopeProvider? ScopeProvider { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerProvider"/> class with the specified options monitor. The provider listens for changes in the options and applies them to the logging configuration, including setting up a Splunk sink if configured.
    /// </summary>
    /// <param name="options">The options monitor that provides the current <see cref="LoggerOptions"/> and listens for changes.</param>
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

    /// <summary>
    /// Gets an action that can be used to write log entries to the configured Splunk sink. If the Splunk sink is not configured, this property will return null. This action can be used to send log entries directly to Splunk without going through the standard logging pipeline.
    /// </summary>
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

    /// <summary>
    /// Creates a new logger instance for the specified category name. If a logger for the given category name already exists, it returns the existing instance; otherwise, it creates a new one and adds it to the internal collection of loggers.
    /// </summary>
    /// <param name="categoryName">The name of the category for the logger.</param>
    /// <returns>A logger instance for the specified category name.</returns>
    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new Logger(name, this));

    /// <summary>
    /// Sets the external scope provider for this logger provider. This method allows the provider to use an external scope provider for managing logging scopes and their associated state. The provided scope provider will be used by all loggers created by this provider.
    /// </summary>
    /// <param name="scopeProvider">The external scope provider to be used by this logger provider.</param>
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
