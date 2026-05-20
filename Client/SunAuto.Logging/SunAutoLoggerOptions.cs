using Microsoft.Extensions.Logging;

namespace SunAuto.Logging;

/// <summary>
/// Configuration options for the SunAuto logging provider.
/// </summary>
public sealed class SunAutoLoggerOptions
{
    /// <summary>
    /// Minimum log level to emit. Defaults to <see cref="LogLevel.Information"/>.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Whether to include scopes in log output.
    /// </summary>
    public bool IncludeScopes { get; set; } = true;

    /// <summary>
    /// Whether to include timestamps in log output.
    /// </summary>
    public bool IncludeTimestamp { get; set; } = true;

    /// <summary>
    /// Format string for timestamps. Defaults to ISO 8601 (UTC).
    /// </summary>
    public string TimestampFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffZ";

    /// <summary>
    /// Custom sink that receives formatted log entries.
    /// When null, output is written to <see cref="Console.Out"/>.
    /// </summary>
    public Action<string>? Sink { get; set; }
}
