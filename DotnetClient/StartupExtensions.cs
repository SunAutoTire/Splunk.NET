using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using SplunkLogger = SunAuto.Logging.Client.Splunk.Storage;
using SunLogger = SunAuto.Logging.Client.StorageService.Storage;

namespace SunAuto.Logging.Client;

public static class StartupExtensions
{
    public static ILoggingBuilder AddSunAutoLogging(this ILoggingBuilder builder, IConfiguration configuration, string sectionName = "Logging:SunAuto")
    {
        builder.AddConfiguration();

        builder.Services.AddSingleton<IStorage, SunLogger>();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerProvider>());


        LoggerProviderOptions.RegisterProviderOptions<LoggerConfiguration, LoggerProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddSplunkLogging(this ILoggingBuilder builder, IConfiguration configuration, string sectionName = "Logging:SunAuto")
    {
        builder.AddConfiguration();

        builder.Services.AddSingleton<IStorage, SplunkLogger>();

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

