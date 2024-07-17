using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SunAuto.Logging.Api.Services;
using System.Text.Json;

namespace SunAuto.Logging.Api;

public class LogQueue(TableClient tableClient, ILoggerFactory loggerFactory)
{
    readonly TableClient TableClient = tableClient;
    readonly ILogger<LogQueue> Logger = loggerFactory.CreateLogger<LogQueue>();

    [Function(nameof(LogQueue))]
    public async Task RunAsync([QueueTrigger("%LogQueueName%", Connection = "LogEntryProcessSas")] QueueMessage message)
    {
        try
        {
            Logger.LogInformation("Queue trigger function processed: {text}", message.MessageText);

            var body = message.Body.ToString();
            var entry = JsonSerializer.Deserialize<Entry>(body);

            await TableClient.AddEntityAsync(entry!, default);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Queue trigger function erred: {text}", message.MessageText);

            throw;
        }
    }
}