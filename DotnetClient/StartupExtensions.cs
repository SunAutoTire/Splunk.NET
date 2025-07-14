using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using SunAuto.Splunk.Client.Splunk;

namespace SunAuto.Splunk.Client;

public static class StartupExtensions
{
    public static ILoggingBuilder AddSplunkLogging(this ILoggingBuilder builder, IConfiguration configuration, string sectionName = "Logging:SunAuto")
    {
        builder.AddConfiguration();

        builder.Services.AddSingleton<IStorage, Storage>();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerProvider>());


        LoggerProviderOptions.RegisterProviderOptions<LoggerConfiguration, LoggerProvider>(builder.Services);

        return builder;
    }
}

