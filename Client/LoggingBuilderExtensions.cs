using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace SunAuto.Logging;

/// <summary>
/// Extension methods for registering the SunAuto logging provider with <see cref="ILoggingBuilder"/>.
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// Adds the SunAuto logging provider using settings from the <c>Logging:SunAuto</c>
    /// configuration section (or <c>appsettings.json</c> equivalents).
    /// </summary>
    public static ILoggingBuilder AddSunAutoLogging(this ILoggingBuilder builder)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));

        builder.AddConfiguration();

        builder
            .Services
            .TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<LoggerOptions, LoggerProvider>(
            builder.Services);

        return builder;
    }

    /// <summary>
    /// Adds the SunAuto logging provider with inline configuration.
    /// </summary>
    public static ILoggingBuilder AddSunAutoLogging(
        this ILoggingBuilder builder,
        Action<LoggerOptions> configure)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        builder.AddSunAutoLogging();
        builder.Services.Configure(configure);

        return builder;
    }
}
