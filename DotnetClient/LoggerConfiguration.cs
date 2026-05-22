using Microsoft.Extensions.Logging;

namespace SunAuto.Splunk.Client;

/// <summary>
/// Configuration options for the SunAuto Splunk logging provider.
/// Bound from the <c>Logging:SunAuto</c> configuration section by the options system.
/// </summary>
public class LoggerConfiguration
{
    /// <summary>
    /// Per-category log level overrides. Use <c>"Default"</c> as the fallback key.
    /// </summary>
    public Dictionary<string, LogLevel> LogLevel { get; set; } = new()
    {
        ["Default"] = Microsoft.Extensions.Logging.LogLevel.Information
    };
}
