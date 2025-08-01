using Microsoft.Extensions.Logging;

namespace SunAuto.Splunk.Client;

/// <summary>
/// Extension methods.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Convert string value to Log level.
    /// </summary>
    /// <param name="levelValue">String value to convert</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Level value is null.</exception>
    /// <exception cref="ArgumentException">Level value does not match a Log Level value.</exception>
    public static LogLevel ToLogLevel(this string? levelValue)
    {
        const string errormessage = "Valid log level must be configured \"Trace\"\r\n\"Debug\"\r\n\"Information\"\r\n\"Warning\"\r\n\"Error\",\r\n\"Critical\",\r\n\"None\"";

        if (string.IsNullOrWhiteSpace(levelValue))
            throw new ArgumentException(errormessage, nameof(levelValue));
        else
            return levelValue switch
            {
                "Trace" => LogLevel.Trace,
                "Debug" => LogLevel.Debug,
                "Information" => LogLevel.Information,
                "Warning" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                "Critical" => LogLevel.Critical,
                "None" => LogLevel.None,
                _ => throw new ArgumentOutOfRangeException(nameof(levelValue), errormessage),
            };
    }

    /// <summary>
    /// Convert value to Event ID
    /// </summary>
    /// <param name="value">Value containing at least an integer value delimited with a colon (:) and a optionally a string value as well.</param>
    /// <returns></returns>
    public static EventId ToEventId(this string value)
    {
        var values = value.Split(':');
        var name = values.Length > 1 ? values[1] : null;
        name = String.IsNullOrWhiteSpace(name) ? null : name;

        return new EventId(Int32.Parse(values[0]), name);
    }
}
