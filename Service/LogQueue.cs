using Azure.Data.Tables;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SunAuto.Logging.Api.Services;
using System.Text.Json;

namespace SunAuto.Logging.Api;

public class LogQueue(TableClient tableClient, ILogger<LogQueue> logger)
{
    readonly TableClient TableClient = tableClient;
    private readonly ILogger<LogQueue> _logger = logger;

    [Function(nameof(LogQueue))]
    public async Task RunAsync([QueueTrigger("logentry", Connection = "UniversalLoggingConnectionString")] QueueMessage message)
    {
        try
        {
            _logger.LogInformation("Queue trigger function processed: {text}", message.MessageText);

            var body = message.Body.ToString();
            var entry = JsonSerializer.Deserialize<Entry>(body);

            await TableClient.AddEntityAsync(entry!, default);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Queue trigger function erred: {text}", message.MessageText);

            throw;
        }
    }
}