using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client.TableStorage;

public class Storage(string environment) : IStorage
{
    private readonly string environment = environment;

    public void Add<TState>(LogLevel logLevel, EventId eventId, TState? state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        throw new NotImplementedException();
    }

    public void Delete(EventId eventId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<LogItem> List()
    {
        throw new NotImplementedException();
    }
}
