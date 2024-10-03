using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client;

[ProviderAlias("SunAuto")]
public sealed class LoggerProvider(Logger logger,IConfiguration configuration) :
    ILoggerProvider
{
    //readonly List<Logger> Loggers = [];
    //readonly Logger Logger = logger;

    public ILogger CreateLogger(string categoryName)
    {
        //var logger = new Logger(configuration);
        //Loggers.Add(logger);

        return logger;
    }

    public void Dispose() { }
}
