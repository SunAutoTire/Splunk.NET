using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SunAuto.Logging;

public class Logger(IConfiguration configuration) : ILogger
{
    private readonly IConfiguration Configuration = configuration;
    private IStorage? Storage;

    IStorage GetStorage()
    {
        if (Storage == null)
        {
            var environment = Environment.GetEnvironmentVariable("LoggingEnvironment");

            Storage = environment switch
            {
                "Development" or "Staging" or "Test" or "Production" => new TableStorage.Storage(environment),
                _ => new FileStorage.Storage(Configuration.GetValue<string>("Logging:SunAuto:Path")!),
            };
        }

        return Storage;
    }

    readonly LogLevel DefaultLevel = configuration.GetValue<string>("Logging:SunAuto:LogLevel:Default").ToLogLevel();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= DefaultLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            try
            {
                GetStorage().Add(logLevel, eventId, state, exception, formatter);
            }
            finally
            {
            }
        }
    }
}
