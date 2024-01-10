using Microsoft.Extensions.Logging;
using SunAuto.Logging.Common;

namespace SunAuto.Logging.FileStorage;

public class Storage(string path) : IStorage
{
    private readonly string Path = path;

    public void Add<TState>(LogLevel logLevel, EventId eventId, TState? state, Exception? exception, Func<TState, Exception?, string> formatter) => File.AppendAllLines(Path, new string[] { $"{logLevel},{eventId},{state},{exception}" });

    public void Delete(EventId eventId) => throw new NotImplementedException();

    public IEnumerable<ILogItem> List() => throw new NotImplementedException();
}
