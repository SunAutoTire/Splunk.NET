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


sealed class SampleWorker(ILogger<SampleWorker> logger, IHostApplicationLifetime lifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
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

                    var innerEx = new InvalidOperationException("Boy this is really messed up", new ApplicationException("Something went wrong"));

                    logger.LogError(innerEx, "Really Failed to charge customer for order {OrderId}", 1042);
                }

                for (int i = 0; i < 100; i++)
                {
                    logger.LogInformation("Processing item {ItemId}", i);
                    await Task.Delay(100, stoppingToken);
                }
            }

            logger.LogWarning("Queue depth is high, consider scaling");
            logger.LogInformation("Worker finished — shutting down");

            await Task.Delay(200, stoppingToken);

            lifetime.StopApplication();   // instead of Environment.Exit(0)

        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Unhandled exception");
            Environment.ExitCode = 1;
            lifetime.StopApplication();
        }
    }
}
