using System;
using System.Threading;
using Azure.Data.Tables;
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
        public async Task Run([TimerTrigger("0 0 0 1 * *",RunOnStartup = true)] TimerInfo myTimer) 
        {
            _logger.LogInformation("PurgeTables function started.");

            try
            {
                var cancellationToken = new CancellationTokenSource().Token;

                // Obtenemos las URIs de cada tabla del archivo de configuración
                var tableUris = new[]
                {
                    new { Environment = "Development", Uri = _configuration["Values:TableSasDevelopment"] },
                    new { Environment = "Staging", Uri = _configuration["Values:TableSasStaging"] },
                    new { Environment = "Production", Uri = _configuration["Values:TableSasProduction"] }
                };

                foreach (var table in tableUris)
                {
                    _logger.LogInformation($"Starting cleanup for {table.Environment} environment.");

                    var tableClient = new TableClient(new Uri(table.Uri));
                    await _utilities.CleanupOldEntriesAsync(tableClient, table.Environment, cancellationToken);

                    _logger.LogInformation($"Cleanup completed for {table.Environment} environment.");
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