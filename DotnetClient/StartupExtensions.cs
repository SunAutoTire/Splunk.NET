using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using SunAuto.Logging.Client.TableStorage;

namespace SunAuto.Logging.Client;

public static class StartupExtensions
{
    public static ILoggingBuilder AddSunAutoLogging(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();
        builder.Services.AddScoped<IStorage, Storage>();
        builder.Services.AddScoped<ILogger, Logger>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerProvider>());


        LoggerProviderOptions.RegisterProviderOptions<LoggerConfiguration, LoggerProvider>(builder.Services);

        return builder;
    }

    //public static ILoggingBuilder AddSunAutoLogging(this ILoggingBuilder builder, Action<LoggerConfiguration> configure)
    //{
    //    builder.AddSunAutoLogging();
    //    builder.Services.Configure(configure);

    //    return builder;
    //}
}

