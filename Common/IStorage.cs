using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Common;

public interface IStorage
{
    void Add<TState>(LogLevel logLevel, EventId eventId, TState? state, Exception? exception, Func<TState, Exception?, string> formatter);

    IEnumerable<ILogItem> List();

    void Delete(EventId eventId);
}
