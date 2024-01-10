using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunAuto.Logging.Common;

namespace SunAuto.Logging;

public class Logger(IStorage log, IConfiguration configuration) : ILogger
{
    readonly IStorage Storage = log;

    readonly LogLevel DefaultLevel = configuration.GetValue<string>("Logging:SunAuto:LogLevel:Default").ToLogLevel();
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new State(state);

    public bool IsEnabled(LogLevel logLevel) => logLevel >= DefaultLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            try
            {
                Storage.Add(logLevel,eventId,state,exception,formatter);
            }
            finally
            {
            }
        }
    }
}
