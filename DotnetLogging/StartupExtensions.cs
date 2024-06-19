using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace SunAuto.Logging.Client;

public static class StartupExtensions
{
    public static ILoggingBuilder AddLogging(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<LoggerConfiguration, LoggerProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddLogging(this ILoggingBuilder builder, Action<LoggerConfiguration> configure)
    {
        builder.AddLogging();
        builder.Services.Configure(configure);

        return builder;
    }
}

