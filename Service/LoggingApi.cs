using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SunAuto.Api.Hateoas;
using SunAuto.Logging.Api.Models;
using System.Net;
using System.Text.Json;
using TableEntry = SunAuto.Logging.Api.Services.Entry;

namespace SunAuto.Logging.Api;

public class LoggingApi(TableClient tableClient, QueueClient queue)
{
    readonly TableClient TableClient = tableClient;
    readonly QueueClient QueueClient = queue;
    readonly string ApplicationName = "UniversalLogging";

    [Function("LoggerItem")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", "delete", Route = "{application:alpha?}/{level:alpha?}")] HttpRequestData req,
                                string? application,
                                string? level,
                                string? next,
                                FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("LoggerItem");
        logger.LogInformation("POST Log item {url}.", req.Url);
        try
        {
            switch (req.Method)
            {
                case "GET":
                    var output = ListAsync(req, next, application, level, default);

                    return await CreateResponseAsync(req, HttpStatusCode.OK, output);
                case "POST":
                    var body = req.Body;
                    await CreateAsync(application, level, body);

                    return await CreateResponseAsync(req, HttpStatusCode.Created, new { Message = "Entry logged." });
                //case "DELETE":
                default:
                    throw new NotImplementedException();
            }

            //var message = String.Format($"Category: {category}, ID: {id}");
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

    public Linked<IEnumerable<Entry>> ListAsync(HttpRequestData req, string? next, string? application, string? level, CancellationToken cancellationToken)
    {
        var applicationfilter = String.IsNullOrWhiteSpace(application) ? null : $"PartitionKey eq '{application}'";
        var levelfilter = String.IsNullOrWhiteSpace(level) ? null : $"Level eq '{level}'";

        var filter = String.Join(" and ", new string?[] { applicationfilter, levelfilter }.Where(i => !String.IsNullOrWhiteSpace(i)));

        var output = TableClient.Query<TableEntry>(filter, 25, null, cancellationToken);

        var page = output
             .AsPages(next)
             .First();

        return new Linked<IEnumerable<Entry>>(page
            .Values
            .Select(i => new Entry
            {
                Application = i.PartitionKey,
                Level = i.Level,
                ETag = i.ETag,
                Message = i.Message,
                RowKey = i.RowKey,
                Timestamp = i.Timestamp,
                Body = JsonSerializer.Deserialize<object>(i.Body),
            }),
            "Entries",
            [new($"{req.Url.LocalPath}?next={page.ContinuationToken}")]);
    }

    static async Task<HttpResponseData> CreateResponseAsync<T>(HttpRequestData req, HttpStatusCode status, T? body)
    {
        var response = req.CreateResponse(status);
        response.Headers.Add("Content-Type", "application/json");

        if (body != null)
        {
            var content = JsonSerializer.Serialize(body);

            await response.WriteStringAsync(content);
        }

        return response;
    }

    internal async Task CreateAsync(string? application, string? level, Stream body)
    {

        if (String.IsNullOrWhiteSpace(application)) throw new ArgumentException("Application must be set in the route.", nameof(application));
        if (String.IsNullOrWhiteSpace(level)) throw new ArgumentException("Level must be set in the route.", nameof(level));

        var reader = new StreamReader(body);
        var bodystring = reader.ReadToEnd();

        var messageandcontent = JsonSerializer.Deserialize<MessageAndContent>(bodystring);

        var entry = new TableEntry
        {
            Application = application,
            Body = JsonSerializer.Serialize(messageandcontent?.Content),
            Level = level,
            PartitionKey = ApplicationName,
            Message = messageandcontent?.Message,
        };

        var message = JsonSerializer.Serialize(entry);

        var receipt = await QueueClient.SendMessageAsync(message);
    }
}
