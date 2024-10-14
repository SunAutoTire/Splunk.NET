using System;
using System.Threading;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
namespace CleanTableLogs
{
    public class PurgeTables
    {
        private readonly ILogger _logger;
        private readonly LogUtilities _utilities;

        public PurgeTables(ILoggerFactory loggerFactory, LogUtilities utilities)
        {
            _logger = loggerFactory.CreateLogger<PurgeTables>();
            _utilities = utilities;
        }


        [Function("PurgeTables")]
        public async Task Run([TimerTrigger("0 0 0 1 * *")] TimerInfo myTimer) 
        {
            _logger.LogInformation("PurgeTables function started.");

            try
            {
                // Cleanup old entries based on the environment
                var environment = Environment.GetEnvironmentVariable("LoggingEnvironment");
                var cancellationToken = new CancellationTokenSource().Token;

                await _utilities.CleanupOldEntriesAsync(environment, cancellationToken);

                _logger.LogInformation("PurgeTables function completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred in PurgeTables: {ex.Message}");
            }
        }
    }
}