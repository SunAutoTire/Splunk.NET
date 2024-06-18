using Azure.Data.Tables;

namespace SunAuto.Logging.Api.Services.LoggingStorage;

public class Logger(TableClient tableClient) : ILogger
{
    readonly TableClient Client = tableClient;

    //public async Task CreateAsync(string application, string environment, string level, Stream body, CancellationToken cancellationToken)
    //{
    //    await Client.AddEntityAsync<Entry>(new Entry
    //    {
    //        Application = application,
    //        Environment = environment,
    //        Level = level,
    //        Body = body,
    //    }, cancellationToken);
    //}

    public async Task CreateAsync(Entry entry, CancellationToken cancellationToken)
    {
        await Client.AddEntityAsync<Entry>(entry, cancellationToken);
    }
}
