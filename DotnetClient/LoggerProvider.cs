using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client;

[ProviderAlias("SunAuto")]
public sealed class LoggerProvider :
    ILoggerProvider
{
    readonly Dictionary<string, ILogger> _loggers = [];

    public ILogger CreateLogger(string categoryName)
    {
        var check = _loggers.TryGetValue(categoryName, out ILogger? dictionarylogger);

        if (check)
            return dictionarylogger!;
        else
        {
            _loggers.Add(categoryName, new Logger(new());

            return _loggers[categoryName];
        }
    }

    public void Dispose() { }
}
