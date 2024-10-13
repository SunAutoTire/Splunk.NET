using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace SunAuto.Logging.Client;

public static class StartupExtensions
{
    public static ILoggingBuilder AddSunAutoLogging(this ILoggingBuilder builder, IConfiguration configuration, string sectionName = "Logging:SunAuto")
    {
        builder.AddConfiguration();

        if (configuration.GetSection(sectionName)["Environment"] == "File")
            builder.Services.AddSingleton<IStorage, FileStorage.Storage>();
        else
            builder.Services.AddScoped<IStorage, TableStorage.Storage>();

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

