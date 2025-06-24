using Azure.Data.Tables;
using CleanTableLogs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        // Add configuration sources within the host builder
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddSingleton<TableClient>(provider =>
            new TableClient(new Uri(configuration["Values:TableSas"])));

        //services.AddSingleton<LogUtilities>();
    })
    .Build();

host.Run();
