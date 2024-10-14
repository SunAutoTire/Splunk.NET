using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CleanTableLogs
{
    public class PurgeTables
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly LogUtilities _utilities;

        public PurgeTables(ILoggerFactory loggerFactory, IConfiguration configuration, LogUtilities logUtilities)
        {
            _logger = loggerFactory.CreateLogger<PurgeTables>();
            _configuration = configuration;
            _utilities = logUtilities;
        }

        [Function("PurgeTables")]
        public async Task Run([TimerTrigger("0 0 0 1 * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            _logger.LogInformation("PurgeTables function started.");

            try
            {
                var cancellationToken = new CancellationTokenSource().Token;

                var environments = new[] { "Development", "Staging", "Production" };

                foreach (var environment in environments)
                {
                    _logger.LogInformation($"Starting cleanup for {environment} environment.");

                    await _utilities.CleanupOldEntriesAsync(environment, cancellationToken);

                    _logger.LogInformation($"Cleanup completed for {environment} environment.");
                }

                _logger.LogInformation("PurgeTables function completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred in PurgeTables: {ex.Message}");
            }
        }
    }
}
