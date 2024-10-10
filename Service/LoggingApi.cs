using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SunAuto.Hateoas;
using SunAuto.Logging.Common;
using System.Net;
using System.Text.Json;
 using TableEntry = SunAuto.Logging.Api.Services.Entry;

namespace SunAuto.Logging.Api;

public class LoggingApi(TableClient tableClient, QueueClient queue, ILoggerFactory loggerFactory)
{
    readonly TableClient TableClient = tableClient;
    readonly QueueClient QueueClient = queue;

    readonly ILogger<LoggingApi> Logger = loggerFactory.CreateLogger<LoggingApi>();

    //[Function("CreateLoggerItems")]
    //public async Task<HttpResponseData> CreateBatchAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "")] HttpRequestData req)
    //{
    //    Logger.LogInformation("POST Log item {url}.", req.Url);

    //    try
    //    {
    //        if (req.Body == null)
    //            return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "Post body must be present.");

    //        var receipt = req.Method switch
    //        {
    //            "POST" => await HandlePostBatchRequest(req.Body),
    //            _ => null
    //        };

    //        return receipt == null
    //            ? await CreateErrorResponseAsync(req, HttpStatusCode.MethodNotAllowed, "Method not allowed.")
    //            : await CreateResponseAsync(req, HttpStatusCode.Created, new { Message = "Entry logged." });

    //    }
    //    catch (ArgumentException ex)
    //    {
    //        return await Logger.HandleErrorAsync(req, ex, HttpStatusCode.NotFound);
    //    }
    //    catch (NullReferenceException ex)
    //    {
    //        return await Logger.HandleErrorAsync(req, ex, HttpStatusCode.BadRequest);
    //    }
    //    catch (InvalidOperationException ex)
    //    {
    //        return await Logger.HandleErrorAsync(req, ex, HttpStatusCode.Conflict);
    //    }
    //    catch (Exception ex)
    //    {
    //        Logger.LogError(ex, "An unhandled exception occurred during processing.");
    //        return await Logger.HandleErrorAsync(req, ex, HttpStatusCode.InternalServerError);
    //    }
    //}

    [Function("CreateLoggerItem")]
    public async Task<HttpResponseData> CreateAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "{application:alpha?}/{Level:alpha?}")] HttpRequestData req,
                                string? application,
                                string? Level)
    {
        Logger.LogInformation("POST Log item {url}.", req.Url);

        try
        {
            return req.Method switch
            {
                "POST" => await HandlePostRequest(req, application, Level),
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

    [Function("DeleteLoggerItem")]
    public async Task<HttpResponseData> DeleteAsync([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "{application:alpha?}/{rowKey:guid}")] HttpRequestData req,
                       string? application,
                       string? rowKeyOrLevel)
    {
        Logger.LogInformation("Delete Log item {url}.", req.Url);

        try
        {
            return req.Method switch
            {
                "DELETE" => await HandleDeleteRequest(req, application, rowKeyOrLevel),
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

    [Function("SearchLogsByDateRange")]
    public async Task<HttpResponseData> SearchByDateRangeAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "logs/date-range")] HttpRequestData req,
        DateTime startDate,
        DateTime endDate,
        string? application,
        string? next)
    {
        Logger.LogInformation("Search logs for application {application} between {startDate} and {endDate}.", application, startDate, endDate);

        return await HandleDateRangeSearchAsync(req, next, application, startDate, endDate);
    }

    [Function("SearchLogsByLevel")]
    public async Task<HttpResponseData> SearchLogsByLevelAsync(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "logs/level")] HttpRequestData req,
    string? application,
    string? level,
    string? next)
    {
        Logger.LogInformation("Search logs for application {application} level {level}.", application, level);

        return await HandleLevelSearchAsync(req, next, application, level);
    }

    //TODO: THis could be merged w/ endpoint on line 56
    [Function("SearchSingleLoggerItem")]
    public async Task<HttpResponseData> GetOrDeleteAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "{application:alpha?}/{rowKey:guid}")] HttpRequestData req,
                               string? application,
                               string? rowKey)
    {
        Logger.LogInformation("Get Log item {url}.", req.Url);

        try
        {
            return req.Method switch
            {
                "GET" => await HandleGetSingleItemRequest(req, application, rowKey),
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


    [Function("SearchLoggerItemsAdvance")]
    public async Task<HttpResponseData> GetAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "search")] HttpRequestData req,
                               string? application,
                               string? Level,
                               DateTime? startDate, DateTime? endDate,
                               string? next)
    {
        Logger.LogInformation("Get Log item {url}.", req.Url);

        try
        {
            return req.Method switch
            {
                "GET" => await HandleGetRequest(req, application, Level, startDate, endDate, next),
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

    private async Task<SendReceipt> HandlePostBatchRequest(Stream body)
    {
        using var reader = new StreamReader(body);
        var bodystring = await reader.ReadToEndAsync();
        var entry = JsonSerializer.Deserialize<TableEntry>(bodystring);
        var message = JsonSerializer.Serialize(entry);

        return await QueueClient.SendMessageAsync(message);
    }

    private async Task<HttpResponseData> HandlePostRequest(HttpRequestData req, string? application, string? level)
    {
        if (String.IsNullOrWhiteSpace(application) && String.IsNullOrWhiteSpace(level))
            await CreateAsync(req.Body);
        else if (String.IsNullOrWhiteSpace(application) || String.IsNullOrWhiteSpace(level))
            return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "Application and rowKey must be provided.");
        else
            await CreateAsync(application!, level!, req.Body);

        return await CreateResponseAsync(req, HttpStatusCode.Created, new { Message = "Entry logged." });
    }

    private async Task CreateAsync(Stream body)
    {
        using var reader = new StreamReader(body);
        var bodystring = await reader.ReadToEndAsync();
        var entry = JsonSerializer.Deserialize<IEnumerable<Entry>>(bodystring);

        foreach (var item in entry!)
        {
            var message = JsonSerializer.Serialize(item);

            await QueueClient.SendMessageAsync(message);
        }
    }

    private async Task<HttpResponseData> HandleDeleteRequest(HttpRequestData req, string? application, string? rowKey)
    {
        if (string.IsNullOrWhiteSpace(application) || string.IsNullOrWhiteSpace(rowKey))
            return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "Application and rowKey must be provided.");

        await DeleteAsync(application, rowKey);
        return await CreateResponseAsync(req, HttpStatusCode.NoContent, new { Message = "Successfully deleted." });
    }

    private async Task<HttpResponseData> HandleGetSingleItemRequest(HttpRequestData req, string? application, string? rowKey)
    {
        if (string.IsNullOrWhiteSpace(application) || string.IsNullOrWhiteSpace(rowKey))
            return await CreateErrorResponseAsync(req, HttpStatusCode.BadRequest, "Application and rowKey must be provided.");

        var output = GetByRowKey(application, rowKey, CancellationToken.None);
        return await CreateResponseAsync(req, HttpStatusCode.OK, output);
    }

    private async Task<HttpResponseData> HandleDateRangeSearchAsync(HttpRequestData req, string? next, string? application, DateTime? startDate, DateTime? endDate)
    {
        var output = ListByDateRangeAsync(req, next, application, startDate, endDate, CancellationToken.None);
        return await CreateResponseAsync(req, HttpStatusCode.OK, output);
    }

    private async Task<HttpResponseData> HandleLevelSearchAsync(HttpRequestData req, string? next, string? application, string? level)
    {
        var output = ListByLevelAsync(req, next, application, level, CancellationToken.None);
        return await CreateResponseAsync(req, HttpStatusCode.OK, output);
    }

    private Linked<IEnumerable<Entry>> ListByLevelAsync(HttpRequestData req, string? next, string? application, string? level, CancellationToken cancellationToken)
    {
        var applications = String.IsNullOrWhiteSpace(application) ? null : application.Split('|', StringSplitOptions.RemoveEmptyEntries);

        var applicationfilter = applications == null || applications.Length == 0
            ? null
            : string.Join(" or ", applications.Select(l => $"PartitionKey eq '{l}'"));

        var levels = String.IsNullOrWhiteSpace(level) ? null : level.Split('|', StringSplitOptions.RemoveEmptyEntries);

        var levelfilter = levels == null || levels.Length == 0
            ? null
            : string.Join(" or ", levels.Select(l => $"Level eq '{l}'"));

        var filters = new[] { applicationfilter, levelfilter }
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
                Body = i.Body == null ? null : JsonSerializer.Deserialize<string>(i.Body),
            }), "Entries", links);
    }

    private Linked<IEnumerable<Entry>> ListByDateRangeAsync(HttpRequestData req, string? next, string? application, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var applications = String.IsNullOrWhiteSpace(application) ? null : application.Split('|', StringSplitOptions.RemoveEmptyEntries);

        var applicationfilter = applications == null || applications.Length == 0
            ? null
            : string.Join(" or ", applications.Select(l => $"PartitionKey eq '{l}'"));

        var dateRangeFilter = BuildDateRangeFilter(startDate, endDate);

        var filters = new[] { applicationfilter, dateRangeFilter }
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
                Body = i.Body == null ? null : JsonSerializer.Deserialize<string>(i.Body),
            }), "Entries", links);
    }

    private Linked<IEnumerable<Entry>> ListAsync(HttpRequestData req, string? next, string? application, string? level, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var applications = String.IsNullOrWhiteSpace(application) ? null : application.Split('|', StringSplitOptions.RemoveEmptyEntries);

        var applicationfilter = applications == null || applications.Length == 0
            ? null
            : string.Join(" or ", applications.Select(l => $"PartitionKey eq '{l}'"));

        var levels = String.IsNullOrWhiteSpace(level) ? null : level.Split('|', StringSplitOptions.RemoveEmptyEntries);

        var levelfilter = levels == null || levels.Length == 0
            ? null
            : string.Join(" or ", levels.Select(l => $"Level eq '{l}'"));

        var dateRangeFilter = BuildDateRangeFilter(startDate, endDate);

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
                Body = i.Body == null ? null : JsonSerializer.Deserialize<string>(i.Body),
            }), "Entries", links);
    }

    private static string? BuildDateRangeFilter(DateTime? startDate, DateTime? endDate)
    {
        if (startDate == null && endDate == null) return null;

        var filters = new List<string>();

        if (startDate != null)
            filters.Add($"Timestamp ge datetime'{startDate.Value:yyyy-MM-ddTHH:mm:ssZ}'");

        if (endDate != null)
            filters.Add($"Timestamp le datetime'{endDate.Value:yyyy-MM-ddTHH:mm:ssZ}'");

        return string.Join(" and ", filters);
    }

    private Entry GetByRowKey(string application, string rowKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rowKey))
        {
            throw new ArgumentException("rowKey must be provided", nameof(rowKey));
        }

        var filter = $"PartitionKey eq '{application}' and RowKey eq '{rowKey}'";

        // Query the table synchronously
        var result = TableClient.Query<TableEntry>(filter, cancellationToken: cancellationToken);

        var entry = result.FirstOrDefault() ?? throw new KeyNotFoundException($"No entry found with RowKey '{rowKey}'");

        return new Entry
        {
            Application = entry.PartitionKey,
            Level = entry.Level,
            Message = entry.Message,
            RowKey = entry.RowKey,
            Timestamp = entry.Timestamp,
            Body = entry.Body == null ? null : JsonSerializer.Deserialize<string>(entry.Body),
        };
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

    async Task CreateAsync(string application, string level, Stream body)
    {
        var reader = new StreamReader(body);
        var bodystring = await reader.ReadToEndAsync();
        var entry = JsonSerializer.Deserialize<TableEntry>(bodystring);

        entry!.Application = application;

        var message = JsonSerializer.Serialize(entry);
        entry.Level = level;

        await QueueClient.SendMessageAsync(message);
    }

    async Task DeleteAsync(string? application, string? rowKey)
    {
        if (string.IsNullOrWhiteSpace(application)) throw new ArgumentException("Application must be set in the route.", nameof(application));
        if (string.IsNullOrWhiteSpace(rowKey)) throw new ArgumentException("RowKey must be set in the route.", nameof(rowKey));

        await TableClient.DeleteEntityAsync(application, rowKey);
    }
}
