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

        internal async Task CleanupOldEntriesAsync(string environment, int purgeDays, string purgeLevels, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting old entries...");

            var time = DateTime.UtcNow - TimeSpan.FromDays(purgeDays);
            var levels = purgeLevels.Split('|');
            var result = 0;

            while (true)
            {
                var tasks = new List<Task>();
                string filter = $@"Timestamp le datetime'{time:yyyy-MM-ddTHH:mm:ssZ}' 
                    and ({string.Join(" or ", levels.Select(level => $"Level eq '{level}'"))})";

                var output = _client.Query<Entry>(filter, 1000, null, cancellationToken);
                var page = output.AsPages().FirstOrDefault();

                if (page == null || !page.Values.Any()) break;

                foreach (var value in page.Values)
                {
                    tasks.Add(_client.DeleteEntityAsync(value, cancellationToken: cancellationToken));
                }

                await Task.WhenAll(tasks);
                result += tasks.Count;
            }

            _logger.LogInformation($"Deleted {result} entries for environment: {environment}.");
        }
    }
}
