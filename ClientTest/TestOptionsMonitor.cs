using Microsoft.Extensions.Options;

namespace SunAuto.Logging.Client.Test;

/// <summary>
/// Minimal <see cref="IOptionsMonitor{TOptions}"/> that hands back a fixed instance, so tests can
/// build a <see cref="LoggerProvider"/> without a DI container.
/// </summary>
internal sealed class TestOptionsMonitor(LoggerOptions value) : IOptionsMonitor<LoggerOptions>
{
    public LoggerOptions CurrentValue { get; } = value;

    public LoggerOptions Get(string? name) => CurrentValue;

    public IDisposable? OnChange(Action<LoggerOptions, string?> listener) => null;
}
