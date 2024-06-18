using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SunAuto.Logging.Api;

public class LogQueue(Services.LoggingStorage.ILogger logClient,ILogger<LogQueue> logger)
{
    readonly Services.LoggingStorage.ILogger Client = logClient;
    private readonly ILogger<LogQueue> _logger = logger;

    [Function(nameof(LogQueue))]
    public async Task RunAsync([QueueTrigger("logentry", Connection = "UniversalLoggingConnectionString")] QueueMessage message)
    {
        try
        {
            _logger.LogInformation("Queue trigger function processed: {text}", message.MessageText);

            var body = message.Body.ToString();
            var entry = JsonSerializer.Deserialize<Services.LoggingStorage.Entry>(body);

            await Client.CreateAsync(entry!, default);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Queue trigger function erred: {text}", message.MessageText);

            throw;
        }
    }
}