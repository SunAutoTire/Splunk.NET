using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunAuto.Logging.Common;

namespace SunAuto.Logging;

[ProviderAlias("SunAuto")]
public sealed class LoggerProvider(IStorage storage, IConfiguration configuration) :
    ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new Logger(storage, configuration);

    public void Dispose() => throw new NotImplementedException();
}
