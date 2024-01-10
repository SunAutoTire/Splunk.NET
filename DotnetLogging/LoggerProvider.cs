using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunAuto.Logging.Common;

namespace SunAuto.Logging;

[ProviderAlias("SunAuto")]
public sealed class LoggerProvider(IConfiguration configuration, IStorage? storage = null) :
    ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new Logger(configuration, storage);

    public void Dispose() { }
}
