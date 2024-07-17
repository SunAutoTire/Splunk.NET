using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SunAuto.Logging.Api;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddLogging();

        var factory = new LoggerFactory();
        var logger = factory.CreateLogger<Program>();

        logger.LogInformation("Starting up.");

        try
        {
            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();

            var environmentvariables = Environment.GetEnvironmentVariables();
            var queueadduri = new Uri(environmentvariables["LogEntryAddQueue"]!.ToString()!);
            var tableclienturi = new Uri(environmentvariables["LogTable"]!.ToString()!);

            services.AddScoped(options => new TableClient(tableclienturi));
            services.AddScoped(options => new QueueClient(queueadduri, new()
            {
                MessageEncoding = QueueMessageEncoding.Base64
            }));
            services.AddScoped<LoggingApi>();
            services.AddScoped<LogQueue>();

            logger.LogInformation("Started up.");

        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Start up failed.");
        }
    })
    .Build();

host.Run();