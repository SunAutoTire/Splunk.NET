using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunAuto.Extensions;

namespace SunAuto.Logging.Client;

public class Logger(IStorage storage, IConfiguration configuration) : ILogger
{
    readonly IStorage Storage = storage;
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
            Storage.Add(logLevel, eventId, state, exception, formatter);
    }

    //#region IDisposable

    //private bool disposedValue;

    //protected virtual void Dispose(bool disposing)
    //{
    //    if (!disposedValue)
    //    {
    //        if (disposing)
    //        {
    //            Storage?.Dispose();
    //        }

    //        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
    //        // TODO: set large fields to null
    //        disposedValue = true;
    //    }
    //}

    //// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    //// ~Logger()
    //// {
    ////     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    ////     Dispose(disposing: false);
    //// }

    //public void Dispose()
    //{
    //    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //    Dispose(disposing: true);
    //    GC.SuppressFinalize(this);
    //}

    //#endregion
}
