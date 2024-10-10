// See https://aka.ms/new-console-template for more information
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SunAuto.Development.Console;
using SunAuto.Development.Library.Banners;
using SunAuto.Logging.Client;
using SunAuto.Logging.Console;

var configuration = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .Build();

//Environment.SetEnvironmentVariable("LoggingEnvironment", "Local");
Environment.SetEnvironmentVariable("LoggingEnvironment", "Development");

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

builder.Logging.ClearProviders();
builder.Logging.AddSunAutoLogging();

services.AddLogging();
services.AddSingleton<Authentication>();
services.AddSingleton<IBanner, BairesDev>();
services.AddSingleton<Welcome>();
services.AddScoped<LogGenerator>();
services.AddScoped<LogUtilities>();

var tableclienturi = new Uri(configuration["TableSas"]!);

services.AddScoped(options => new TableClient(tableclienturi));

using var host = builder.Build();

try
{
    var authentication = host.Services.GetService<Authentication>();
    var welcome = host.Services.GetService<Welcome>();

    var result = await authentication!.LogInAsync();
    var roll = welcome!.RollAsync(result!);
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationtoken = cancellationTokenSource.Token;
    await roll;


    await ChooseAsync();

    async Task ChooseAsync()
    {
        Console.WriteLine("What are we up for today?" + Environment.NewLine);
        Console.WriteLine("a - Rename partition key." + Environment.NewLine);
        Console.WriteLine("b - Generate log entries." + Environment.NewLine);
        Console.WriteLine("c - Purge old entries." + Environment.NewLine);
        Console.WriteLine("x - Exit" + Environment.NewLine);

        var input = Console.ReadLine();
        var utilities = host.Services.GetRequiredService<LogUtilities>();

        switch (input)
        {
            case ("a"):
                var tasks = new List<Task>{
                utilities.RenamePartitionKeysAsync( "ABDGTireData", "ABDGTireServiceAPI", cancellationtoken),
                utilities.RenamePartitionKeysAsync( "ABDGTireDataAPI", "ABDGTireServiceAPI", cancellationtoken),
                utilities.RenamePartitionKeysAsync( "CarifyAPI", "CarifyBusinessAPI", cancellationtoken),
                utilities.RenamePartitionKeysAsync( "SatsIntegrationAPI", "SatsIntSatsMainServiceAPI", cancellationtoken),
                utilities.RenamePartitionKeysAsync( "TireData", "TireDataServiceAPI", cancellationtoken),
                utilities.RenamePartitionKeysAsync( "TireDataServices", "TireDataServiceAPI", cancellationtoken),
                utilities.RenamePartitionKeysAsync( "VastOfficeAPI", "VastVastOfficeServiceApi", cancellationtoken),
                utilities.RenamePartitionKeysAsync( "WebpageResourcesAPI", "WebResourcesServiceAPI", cancellationtoken),
                utilities.RenamePartitionKeysAsync( "VastVastOfficeAPI", "VastVastOfficeServiceApi", cancellationtoken),
                };

                await Task.WhenAll(tasks);

                break;
            case ("b"):
                var generator = host.Services.GetRequiredService<LogGenerator>();
                generator.Run();
                break;
            case ("c"):
                await utilities.CleanupOldEntriesAsync(cancellationtoken);
                break;
            case ("x"):
                //Environment.Exit(0);
                cancellationTokenSource?.Cancel();
                return;
            default:
                Console.WriteLine();
                Console.WriteLine(Environment.NewLine + "Please choose from the list:" + Environment.NewLine);
                Console.WriteLine();
                await ChooseAsync();
                break;
        }

        // THis should be handled a better way to avoid StackOverflow.com
        await ChooseAsync();
    }

    await host.RunAsync(cancellationtoken);

    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(Environment.NewLine + ex.Message + Environment.NewLine);
    Console.WriteLine("Later!" + Environment.NewLine);
}