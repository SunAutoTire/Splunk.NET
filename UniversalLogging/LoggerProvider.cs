using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SunAuto.UniversalLogging;

[ProviderAlias("SunAuto")]
public sealed class LoggerProvider (IConfiguration configuration):
    ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) =>new Logger(configuration);

    public void Dispose() => throw new NotImplementedException();
}
