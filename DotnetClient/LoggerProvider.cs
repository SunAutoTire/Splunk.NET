using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SunAuto.Splunk.Client;

[ProviderAlias("SunAuto")]
public sealed class LoggerProvider(IStorage storage, IConfiguration configuration) :
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
            _loggers.Add(categoryName, new Logger(storage, configuration));

            return _loggers[categoryName];
        }
    }

    public void Dispose() { }// => storage.Dispose();
}
