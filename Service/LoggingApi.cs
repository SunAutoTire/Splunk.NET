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
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", "delete", Route = "{application:alpha?}/{rowKey?}")] HttpRequestData req,
                                string? application,
                                string? level,
                                DateTime? startDate, DateTime? endDate,
                                string? rowKey,
                                string? next)
    {
        Logger.LogInformation("POST Log item {url}.", req.Url);

        try
        {
            return req.Method switch
            {
                "GET" => await HandleGetRequest(req, application, level, startDate, endDate, next),
                "POST" => await HandlePostRequest(req, application, level),
                "DELETE" => await HandleDeleteRequest(req, application, rowKey),
                _ => await CreateErrorResponseAsync(req, HttpStatusCode.MethodNotAllowed, "Method not allowed.")
            };
        }
        catch (ArgumentException ex)
        {
            return await Logger.HandleErrorAsync(req, ex, HttpStatusCode.NotFound);
        }
        catch (NullReferenceException ex)
        {
            return await Logger.HandleErrorAsync(req, ex, HttpStatusCode.BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            return await Logger.HandleErrorAsync(req, ex, HttpStatusCode.Conflict);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An unhandled exception occurred during processing.");
            return await Logger.HandleErrorAsync(req, ex, HttpStatusCode.InternalServerError);
        }
    }

    private async Task<HttpResponseData> HandleGetRequest(HttpRequestData req, string? application, string? level, DateTime? startDate, DateTime? endDate, string? next)
    {
        var output = ListAsync(req, next, application, level, startDate, endDate, CancellationToken.None);
        return await CreateResponseAsync(req, HttpStatusCode.OK, output);
    }

    private async Task<HttpResponseData> HandlePostRequest(HttpRequestData req, string? application, string? level)
    {
        if (string.IsNullOrWhiteSpace(application) || string.IsNullOrWhiteSpace(level))
            return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "Application and level must be provided.");

        await CreateAsync(application, level, req.Body);
        return await CreateResponseAsync(req, HttpStatusCode.Created, new { Message = "Entry logged." });
    }

    private async Task<HttpResponseData> HandleDeleteRequest(HttpRequestData req, string? application, string? rowKey)
    {
        if (string.IsNullOrWhiteSpace(application) || string.IsNullOrWhiteSpace(rowKey))
            return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "Application and rowKey must be provided.");

        await DeleteAsync(application, rowKey);
        return await CreateResponseAsync(req, HttpStatusCode.NoContent, new { Message = "Successfully deleted." });
    }

    private Linked<IEnumerable<Entry>> ListAsync(HttpRequestData req, string? next, string? application, string? level, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var applicationfilter = String.IsNullOrWhiteSpace(application) ? null : $"PartitionKey eq '{application}'";
        
        var levels = String.IsNullOrWhiteSpace(level) ? null : level.Split('|', StringSplitOptions.RemoveEmptyEntries);
        
        var levelfilter = levels == null || levels.Length == 0
            ? null
            : string.Join(" or ", levels.Select(l => $"Level eq '{l}'"));

        var dateRangeFilter = BuildDateRangeFilter(startDate, endDate);

        //var filter = String.Join(" and ", new string?[] { applicationfilter, levelfilter,dateRangeFilter }.Where(i => !String.IsNullOrWhiteSpace(i)));

        var filters = new[] { applicationfilter, levelfilter, dateRangeFilter }
        .Where(f => !String.IsNullOrWhiteSpace(f))
        .ToArray();

        var filter = String.Join(" and ", filters);

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

    private string? BuildDateRangeFilter(DateTime? startDate, DateTime? endDate)
    {
        if (startDate == null && endDate == null) return null;

        var filters = new List<string>();

        if (startDate != null)
            filters.Add($"Timestamp ge datetime'{startDate.Value:yyyy-MM-ddTHH:mm:ssZ}'");

        if (endDate != null)
            filters.Add($"Timestamp le datetime'{endDate.Value:yyyy-MM-ddTHH:mm:ssZ}'");

        return string.Join(" and ", filters);
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

    private static async Task<HttpResponseData> CreateErrorResponseAsync(HttpRequestData req, HttpStatusCode status, string message)
    {
        var response = req.CreateResponse(status);
        response.Headers.Add("Content-Type", "application/json");
        var content = JsonSerializer.Serialize(new { Message = message });
        await response.WriteStringAsync(content);
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

    async Task DeleteAsync(string? application, string? rowKey)
    {
        if (string.IsNullOrWhiteSpace(application)) throw new ArgumentException("Application must be set in the route.", nameof(application));
        if (string.IsNullOrWhiteSpace(rowKey)) throw new ArgumentException("RowKey must be set in the route.", nameof(rowKey));

        await TableClient.DeleteEntityAsync(application, rowKey);
    }
}
