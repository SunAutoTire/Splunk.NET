using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SunAuto.Logging.Client;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSunAutoLogging();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<SampleWorker>();
    })
    .Build();

await host.RunAsync();

sealed class SampleWorker(ILogger<SampleWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker started");

        using (logger.BeginScope("ProcessOrder {OrderId}", 1042))
        {
            logger.LogDebug("Fetching order details");
            logger.LogInformation("Order authorised");

            try
            {
                throw new InvalidOperationException("Payment gateway timeout");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to charge customer for order {OrderId}", 1042);
            }
        }

        logger.LogWarning("Queue depth is high, consider scaling");
        logger.LogInformation("Worker finished — shutting down");

        await Task.Delay(200, stoppingToken);

        Environment.Exit(0);
    }
}
