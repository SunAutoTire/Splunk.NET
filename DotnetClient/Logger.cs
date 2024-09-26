using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunAuto.Extensions;

namespace SunAuto.Logging.Client;

public class Logger(IConfiguration configuration) : ILogger
{
    readonly IConfiguration Configuration = configuration;
    IStorage? Storage;

    IStorage GetStorage()
    {
        if (Storage == null)
        {
            var configuration = Configuration.GetSection("Logging:SunAuto");
            var environment = configuration.GetValue<string>("Environment");

            var environmentname = String.IsNullOrWhiteSpace(environment)
                ? Environment.GetEnvironmentVariable("LoggingEnvironment")
                : environment;

            Storage = environmentname switch
            {
                "Development" or "Staging" or "Test" or "Production" => new TableStorage.Storage(configuration),
                _ => new FileStorage.Storage(configuration.GetValue<string>("Path")!),
            };
        }

        return Storage;
    }

    readonly LogLevel DefaultLevel = GetLogLevel(configuration);

    static LogLevel GetLogLevel(IConfiguration configuration)
    {
        try
        {
            return configuration.GetValue<string>("Logging:SunAuto:LogLevel:Default").ToLogLevel();
        }
        catch (ArgumentException ex)
        {
            var message = typeof(Logger).GetEmbeddedResource("SunAuto.Logging.Client.ConfigurationExMessage.txt");
            throw new InvalidOperationException(message, ex);
        }
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= DefaultLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
            GetStorage().Add(logLevel, eventId, state, exception, formatter);
    }
}
