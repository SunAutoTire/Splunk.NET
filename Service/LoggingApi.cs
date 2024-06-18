using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using ILogClient = SunAuto.Logging.Api.Services.LoggingStorage.ILogger;

namespace SunAuto.Logging.Api;

public class LoggingApi(QueueClient queue, ILogClient logClient, ILoggerFactory loggerFactory)
{
    readonly QueueClient QueueClient = queue;
    readonly ILogClient LogClient = logClient;
    readonly string ApplicationName = "UniversalLogging";
    readonly string Environment = "Development";
    readonly ILogger<LogQueue> _logger = loggerFactory.CreateLogger<LogQueue>();

    [Function("LoggerItem")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", "delete", Route = "{application:alpha?}/{environment:alpha?}/{level:alpha?}")] HttpRequestData req,
                                string? application,
                                string? environment,
                                string? level,
                                FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("LoggerItem");
        logger.LogInformation("POST Log item {url}.", req.Url);
        try
        {
            switch (req.Method)
            {
                //case "GET":
                //    break;
                case "POST":
                    var body = req.Body;
                    await CreateAsync(application, level, body);
                    break;
                //case "DELETE":
                default:
                    throw new NotImplementedException();
            }

            //var message = String.Format($"Category: {category}, ID: {id}");
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");


            //var content = JsonSerializer.Serialize(new { Message = message });
            //response.WriteString(content);

            return response;
        }
        catch (ArgumentException ex)
        {
            return logger.HandleError(req, ex, HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            return logger.HandleError(req, ex, HttpStatusCode.InternalServerError);
        }
    }

    async Task CreateAsync(string? application, string? level, Stream body)
    {

        if (String.IsNullOrWhiteSpace(application)) throw new ArgumentException("Application must be set in the route.", nameof(application));
        if (String.IsNullOrWhiteSpace(level)) throw new ArgumentException("Level must be set in the route.", nameof(level));

        var reader = new StreamReader(body);
        var content = reader.ReadToEnd();

        var entry = new Services.LoggingStorage.Entry
        {
            Application = application,
            Body = content,
            Environment = Environment,
            Level = level,
            PartitionKey = ApplicationName,
        };

        var message = JsonSerializer.Serialize(entry);

    var receipt =     await QueueClient.SendMessageAsync(message);

    }
}
