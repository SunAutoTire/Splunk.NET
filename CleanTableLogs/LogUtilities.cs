using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace CleanTableLogs
{
    public class LogUtilities
    {
        private readonly TableClient _client;
        private readonly ILogger<LogUtilities> _logger;

        public LogUtilities(TableClient client, ILogger<LogUtilities> logger)
        {
            _client = client;
            _logger = logger;
        }

        internal async Task CleanupOldEntriesAsync(string? environment, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Deleting old entries for environment: {environment}...");

            var check = true;
            var result = 0;
            var time = DateTime.UtcNow - TimeSpan.FromDays(30);

            while (check)
            {
                var tasks = new List<Task>();
                string filter = environment switch
                {
                    "Production" => $"Timestamp le datetime'{time:yyyy-MM-ddTHH:mm:ssZ}' and (Level eq 'Debug' or Level eq 'Trace')",
                    _ => $"Timestamp le datetime'{time:yyyy-MM-ddTHH:mm:ssZ}'"
                };

                var output = _client.Query<Entry>(filter, 1000, null, cancellationToken);
                var page = output.AsPages().FirstOrDefault();

                if (page == null || !page.Values.Any()) break;

                foreach (var value in page.Values)
                {
                    tasks.Add(_client.DeleteEntityAsync(entity: value, cancellationToken: cancellationToken));
                }

                await Task.WhenAll(tasks);
                result += tasks.Count;
            }

            _logger.LogInformation($"Deleted {result} entries for environment: {environment}.");
        }
    }
}
