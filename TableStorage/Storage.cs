using Microsoft.Extensions.Logging;
using SunAuto.Logging.Common;

namespace SunAuto.Logging.TableStorage;

public class Storage : IStorage
{
    private string environment;

    public Storage(string environment)
    {
        this.environment = environment;
    }

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
