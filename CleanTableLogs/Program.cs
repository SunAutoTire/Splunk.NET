using Azure.Data.Tables;
using CleanTableLogs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var environment = Environment.GetEnvironmentVariable("LoggingEnvironment") ?? "Development";
var tableSasUri = configuration[$"Values:{environment}:TableSas"];

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddScoped(options => new TableClient(new Uri(tableSasUri)));
        services.AddScoped<LogUtilities>();
    })
    .Build();

host.Run();
