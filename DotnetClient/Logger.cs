using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SunAuto.Splunk.Client;

/// <summary>
/// An <see cref="ILogger"/> implementation that forwards log entries to <see cref="IStorage"/>
/// for async delivery to Splunk. Log level filtering is driven by
/// <c>Logging:SunAuto:LogLevel:Default</c> in application configuration.
/// </summary>
public class Logger(IStorage storage, IConfiguration configuration) : ILogger
{
    readonly IStorage Storage = storage;
    readonly LogLevel DefaultLevel = GetLogLevel(configuration);

    /// <summary>
    /// Reads and parses the configured default log level.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured <see cref="LogLevel"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <c>Logging:SunAuto:LogLevel:Default</c> value is missing or invalid.
    /// </exception>
    static LogLevel GetLogLevel(IConfiguration configuration)
    {
        try
        {
            return configuration.GetValue<string>("Logging:SunAuto:LogLevel:Default").ToLogLevel();
        }
        catch (ArgumentException ex)
        {
            var message = GetLogExceptionMessage();
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <summary>
    /// Builds a human-readable error message describing the required configuration structure.
    /// </summary>
    static string GetLogExceptionMessage()
    {
        var output = new StringBuilder();

        output.AppendLine("SunAuto.Logging requires the following JSON to be added to the \"Logging\" object in the appsettings.json");
        output.AppendLine("e.g.,");
        output.AppendLine();
        output.AppendLine("\"SunAuto\": {");
        output.AppendLine("	\"Source\": <SourceName>,");
        output.AppendLine("	\"Token\": <Splunk HEC Token>,");
        output.AppendLine("	\"BaseUrl\": <e.g. http://splunk-host:8088>,");
        output.AppendLine("	\"LogLevel\": {");
        output.AppendLine("		\"Default\": <Trace, Debug, Information, Warning, Error, Critical, or None>,");
        output.AppendLine("		\"Microsoft.Hosting\": <Trace, Debug, etc.>");
        output.AppendLine("	}");
        output.AppendLine("}");

        return output.ToString();
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="logLevel"/> is at or above the
    /// configured default level.
    /// </summary>
    /// <param name="logLevel">The level to test.</param>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= DefaultLevel;

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
            Storage.Add(logLevel, eventId, state, exception, formatter);
    }
}
