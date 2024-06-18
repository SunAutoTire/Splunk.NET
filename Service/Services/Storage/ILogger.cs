namespace SunAuto.Logging.Api.Services.LoggingStorage;

public interface ILogger
{
    Task CreateAsync(Entry entry, CancellationToken cancellationToken);
}
