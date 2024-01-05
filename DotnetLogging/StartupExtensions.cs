using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace SunAuto.Logging;

public static class StartupExtensions
{
    public static ILoggingBuilder AddLogging(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<LoggerConfiguration, LoggerProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddLogger(this ILoggingBuilder builder, Action<LoggerConfiguration> configure)
    {
        builder.AddLogging();
        builder.Services.Configure(configure);

        return builder;
    }

    internal static LogLevel ToLogLevel(this string? levelValue)
    {
        const string errormessage = "Valid log level must be configured \"Trace\"\r\n\"Debug\"\r\n\"Information\"\r\n\"Warning\"\r\n\"Error\",\r\n\"Critical\",\r\n\"None\"";

        if (string.IsNullOrWhiteSpace(levelValue))
            throw new ArgumentNullException(nameof(levelValue), errormessage);
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
                _ => throw new ArgumentException(errormessage, nameof(levelValue)),
            };
    }
}

