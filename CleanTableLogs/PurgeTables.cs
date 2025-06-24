using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CleanTableLogs
{
    //public class PurgeTables(ILoggerFactory loggerFactory, IConfiguration configuration, LogUtilities logUtilities)
    //{
    //    private readonly ILogger _logger = loggerFactory.CreateLogger<PurgeTables>();

    //    [Function("PurgeTables")]
    //    public async Task Run([TimerTrigger("0 0 0 1 * *", RunOnStartup = true)] TimerInfo myTimer)
    //    {
    //        _logger.LogInformation("PurgeTables function started.");

    //        try
    //        {
    //            var environment = configuration["Values:LoggingEnvironment"];
    //            var purgeDays = int.Parse(configuration["Values:PurgeDays"]);
    //            var purgeLevels = configuration["Values:PurgeLevels"];

    //            _logger.LogInformation($"Starting cleanup");

    //            await logUtilities.CleanupOldEntriesAsync(environment, purgeDays, purgeLevels, default);

    //            _logger.LogInformation($"Cleanup completed");

    //            _logger.LogInformation("PurgeTables function completed.");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError($"An error occurred in PurgeTables: {ex.Message}");
    //        }
    //    }
    //}
}
