using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client.TableStorage
{
    public class QueueEntry<TState>
    {
        public LogLevel Loglevel { get;  set; }
        public EventId EventId { get;  set; }
        public TState? State { get;  set; }
        public string? Formatted { get;  set; }
        public Exception? Exception { get;  set; }
    }
}