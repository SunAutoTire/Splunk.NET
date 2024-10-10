using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SunAuto.Logging.Common;
using System.Text.Json;
using TableEntry = SunAuto.Logging.Api.Services.Entry;

namespace SunAuto.Logging.Api;

public class LogQueue(TableClient tableClient, ILoggerFactory loggerFactory)
{
    readonly TableClient TableClient = tableClient;
    readonly ILogger<LogQueue> Logger = loggerFactory.CreateLogger<LogQueue>();

    [Function(nameof(LogQueue))]
    public async Task RunAsync([QueueTrigger("%LogQueueName%", Connection = "QueueSas")] QueueMessage message)
    {
        try
        {
            Logger.LogInformation("Queue trigger function processed: {text}", message.MessageText);

            var body = message.Body.ToString();
            var entry = JsonSerializer.Deserialize<Entry>(body);

            if (entry == null)
                throw new InvalidOperationException();
            else
            {
                var tableentry = new TableEntry()
                {
                    Body = entry.Body,
                    Level = entry.Level,
                    Message = entry.Message,
                    PartitionKey = entry.Application,
                    RowKey = entry.RowKey,
                    Timestamp = entry.Timestamp,
                };

                await TableClient.AddEntityAsync(tableentry, default);
            }
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Queue trigger function erred: {text}", message.MessageText);

            throw;
        }
    }
}