using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SunAuto.Hateoas;
using SunAuto.Logging.Api.Models;
using System.Net;
using System.Text.Json;
using TableEntry = SunAuto.Logging.Api.Services.Entry;

namespace SunAuto.Logging.Api;

public class LoggingApi(TableClient tableClient, QueueClient queue, ILoggerFactory loggerFactory)
{
    readonly TableClient TableClient = tableClient;
    readonly QueueClient QueueClient = queue;

    readonly ILogger<LoggingApi> Logger = loggerFactory.CreateLogger<LoggingApi>();

    [Function("LoggerItem")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", "delete", Route = "{application:alpha?}/{level:alpha?}")] HttpRequestData req,
                                string? application,
                                string? level,
                                string? next)
    {
        Logger.LogInformation("POST Log item {url}.", req.Url);

        try
        {
            var queryParameters = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var applications = queryParameters.GetValues("application");
            var levels = queryParameters.GetValues("level");

            // Combina parámetros de ruta y consulta
            application ??= applications?.FirstOrDefault();
            level ??= levels?.FirstOrDefault();

            switch (req.Method)
            {
                case "GET":
                    var output = ListAsync(req, next, application, level, default);
                    
                    return output.Object.Any()
                        ? await CreateResponseAsync(req, HttpStatusCode.OK, output)
                        : await CreateResponseAsync(req, HttpStatusCode.NotFound, new { Message = $"No entries found for that application: {application}." });
                case "POST":
                    var body = req.Body;
                    await CreateAsync(application, level, body);

                    return await CreateResponseAsync(req, HttpStatusCode.Created, new { Message = "Entry logged." });
                //case "DELETE":
                default:
                    throw new NotImplementedException();
            }
        }
        catch (ArgumentException ex)
        {
            return await Logger.HandleErrorAsync(req, ex, HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            return await Logger.HandleErrorAsync(req, ex, HttpStatusCode.InternalServerError);
        }
    }

    Linked<IEnumerable<Entry>> ListAsync(HttpRequestData req, string? next, string? application, string? level, CancellationToken cancellationToken)
    {
        var applicationfilter = String.IsNullOrWhiteSpace(application) ? null : $"PartitionKey eq '{application}'";
        var levelfilter = String.IsNullOrWhiteSpace(level) ? null : $"Level eq '{level}'";

        var filter = String.Join(" and ", new string?[] { applicationfilter, levelfilter }.Where(i => !String.IsNullOrWhiteSpace(i)));

        var output = TableClient.Query<TableEntry>(filter, 25, null, cancellationToken);

        var page = output
             .AsPages(next)
             .First();

        var links = new List<Link> { new(req.Url.PathAndQuery) };

        if (!String.IsNullOrWhiteSpace(page.ContinuationToken))
            links.Add(new($"{req.Url.LocalPath}?next={page.ContinuationToken}", "next"));

        return new Linked<IEnumerable<Entry>>(page
            .Values
            .Select(i => new Entry
            {
                Application = i.PartitionKey,
                Level = i.Level,
                Message = i.Message,
                RowKey = i.RowKey,
                Timestamp = i.Timestamp,
                Body = i.Body == null ? null : JsonSerializer.Deserialize<object>(i.Body),
            }), "Entries", links);
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

    async Task CreateAsync(string? application, string? level, Stream body)
    {
        if (String.IsNullOrWhiteSpace(application)) throw new ArgumentException("Application must be set in the route.", nameof(application));
        if (String.IsNullOrWhiteSpace(level)) throw new ArgumentException("Level must be set in the route.", nameof(level));

        var reader = new StreamReader(body);
        var bodystring = await reader.ReadToEndAsync();
        var entry = JsonSerializer.Deserialize<TableEntry>(bodystring);

        entry!.Application = application;

        var message = JsonSerializer.Serialize(entry);

        await QueueClient.SendMessageAsync(message);
    }
}
