using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SunAuto.Logging.Api;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var environmentvariables = Environment.GetEnvironmentVariables();
        var connectionstring = environmentvariables["ConnectionStrings:UniversalLoggingConnectionString"]!.ToString();
        var environment = environmentvariables["AZURE_FUNCTIONS_ENVIRONMENT"];

        services.AddScoped(options => new QueueClient(connectionstring, "logentry", new() { MessageEncoding = QueueMessageEncoding.Base64 }));
        services.AddScoped(options => new TableClient(connectionstring, "Development"));
        services.AddScoped<LoggingApi>();
        services.AddScoped<LogQueue>();
    })
    .Build();

host.Run();
