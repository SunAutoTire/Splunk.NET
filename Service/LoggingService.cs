using Azure.Data.Tables;
using SunAuto.Logging.Api.Models;
using SunAuto.Hateoas;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TableEntry = SunAuto.Logging.Api.Services.Entry;

namespace SunAuto.Logging.Api.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly TableClient _tableClient;
        private readonly IPaginationService _paginationService;

        public LoggingService(TableClient tableClient, IPaginationService paginationService)
        {
            _tableClient = tableClient;
            _paginationService = paginationService;
        }

        public async Task<Linked<IEnumerable<Models.Entry>>> ListAsync(
            string? next,
            string? application,
            string? level,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate,
            CancellationToken cancellationToken)
        {
            var filter = BuildFilter(application, level, startDate, endDate);
            var output = _tableClient.Query<TableEntry>(filter, 25, null, cancellationToken);

            var page = output.AsPages(next).FirstOrDefault();
            var links = _paginationService.GeneratePaginationLinks("/api/logs", page?.ContinuationToken);

            var entries = page?.Values.Select(i => new Models.Entry
            {
                Application = i.PartitionKey,
                Level = i.Level,
                Message = i.Message,
                RowKey = i.RowKey,
                Timestamp = i.Timestamp,
                Body = i.Body == null ? null : JsonSerializer.Deserialize<object>(i.Body),
            }) ?? Enumerable.Empty<Models.Entry>();

            return new Linked<IEnumerable<Models.Entry>>(entries, "Entries", links);
        }

        public async Task<string> ExportLogsAsync(
            string? application,
            string? level,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate,
            CancellationToken cancellationToken)
        {
            var filter = BuildFilter(application, level, startDate, endDate);
            var output = _tableClient.Query<TableEntry>(filter, null, null, cancellationToken);

            var entries = output.Select(i => new Models.Entry
            {
                Application = i.PartitionKey,
                Level = i.Level,
                Message = i.Message,
                RowKey = i.RowKey,
                Timestamp = i.Timestamp,
                Body = i.Body == null ? null : JsonSerializer.Deserialize<object>(i.Body),
            });

            return JsonSerializer.Serialize(entries);
        }

        private string BuildFilter(
            string? application,
            string? level,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
        {
            var applicationFilter = string.IsNullOrWhiteSpace(application) ? null : $"PartitionKey eq '{application}'";
            var levelFilter = string.IsNullOrWhiteSpace(level) ? null : $"Level eq '{level}'";
            var dateFilter = BuildDateFilter(startDate, endDate);

            var filters = new List<string> { applicationFilter, levelFilter, dateFilter }
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToArray();

            return string.Join(" and ", filters);
        }

        private string BuildDateFilter(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            var filters = new List<string>();

            if (startDate.HasValue)
                filters.Add($"Timestamp ge datetime'{startDate.Value.ToString("o")}'");

            if (endDate.HasValue)
                filters.Add($"Timestamp le datetime'{endDate.Value.ToString("o")}'");

            return string.Join(" and ", filters);
        }
    }
}
