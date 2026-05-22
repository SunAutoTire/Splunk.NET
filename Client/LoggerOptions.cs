using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client;

/// <summary>
/// Configuration options for the SunAuto logging provider.
/// </summary>
public sealed class LoggerOptions
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
    /// When null and Splunk options are not set, output goes to <see cref="Console.Out"/>.
    /// </summary>
    public Action<string>? Sink { get; set; }

    /// <summary>
    /// Splunk HEC connection settings. When all three sub-properties are set,
    /// log entries are posted to Splunk automatically.
    /// </summary>
    public SplunkOptions? Splunk { get; set; }

    /// <summary>
    /// Splunk HTTP Event Collector connection settings.
    /// </summary>
    public sealed class SplunkOptions
    {
        /// <summary>
        /// Base URL of the Splunk HEC endpoint (e.g. <c>https://splunk-host:8088/</c>).
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// HEC token used for authentication.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Splunk sourcetype assigned to every event.
        /// </summary>
        public string? Source { get; set; }
    }
}
