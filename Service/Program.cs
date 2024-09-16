using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SunAuto.Logging.Api;
using SunAuto.Logging.Api.Services;

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
            var queueadduri = new Uri(environmentvariables["QueueAddSas"]!.ToString()!);
            var tableclienturi = new Uri(environmentvariables["TableSas"]!.ToString()!);

            services.AddScoped(options => new TableClient(tableclienturi));
            services.AddScoped(options => new QueueClient(queueadduri, new()
            {
                MessageEncoding = QueueMessageEncoding.Base64
            }));

            // Register LoggingApi and LogQueue
            services.AddScoped<LoggingApi>();
            services.AddScoped<LogQueue>();

            // Register LoggingService and IPaginationService
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<IPaginationService, PaginationService>();

            logger.LogInformation("Started up.");

        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Start up failed.");
        }
    })
    .Build();

host.Run();