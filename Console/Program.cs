// See https://aka.ms/new-console-template for more information
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SunAuto.Development.Console;
using SunAuto.Development.Library.Banners;
using SunAuto.Logging.Client;
using SunAuto.Logging.Console;
using System.Threading;

var configuration = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .Build();

var environment = DetermineEnvironment();
var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

AddServices(builder, environment, configuration);

using var host = builder.Build();

try
{
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationtoken = cancellationTokenSource.Token;

    await AuthenticateAndWelcomeAsync(cancellationtoken);
    await ChooseAsync(0, cancellationtoken);

    await host.RunAsync(cancellationtoken);
}
catch (Exception ex)
{
    Console.WriteLine(Environment.NewLine + ex.Message + Environment.NewLine);
    Console.WriteLine("Later!" + Environment.NewLine);
}

async Task<bool> ChooseAsync(short tries = 0, CancellationToken cancellationtoken = default)
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
        case "a":
            var tasks = new List<Task>
                {
                    utilities.RenamePartitionKeysAsync("ABDGTireData", "ABDGTireServiceAPI", cancellationtoken),
                    utilities.RenamePartitionKeysAsync("ABDGTireDataAPI", "ABDGTireServiceAPI", cancellationtoken),
                    utilities.RenamePartitionKeysAsync("CarifyAPI", "CarifyBusinessAPI", cancellationtoken),
                    utilities.RenamePartitionKeysAsync("SatsIntegrationAPI", "SatsIntSatsMainServiceAPI", cancellationtoken),
                    utilities.RenamePartitionKeysAsync("TireData", "TireDataServiceAPI", cancellationtoken),
                    utilities.RenamePartitionKeysAsync("TireDataServices", "TireDataServiceAPI", cancellationtoken),
                    utilities.RenamePartitionKeysAsync("VastOfficeAPI", "VastVastOfficeServiceAPI", cancellationtoken),
                    utilities.RenamePartitionKeysAsync("WebpageResourcesAPI", "WebResourcesServiceAPI", cancellationtoken),
                    utilities.RenamePartitionKeysAsync("VastVastOfficeAPI", "VastVastOfficeServiceAPI", cancellationtoken),
                    utilities.RenamePartitionKeysAsync("VastVastOfficeServiceApi", "VastVastOfficeServiceAPI", cancellationtoken),
                };

            await Task.WhenAll(tasks);

            break;
        case "b":
            var generator = host.Services.GetRequiredService<LogGenerator>();
            generator.Run();
            break;
        case "c":
            await utilities.CleanupOldEntriesAsync(cancellationtoken);
            break;
        case "x":
            //Environment.Exit(0);
            return true;
        default:
            Console.WriteLine();
            if (tries < 3)
                Console.WriteLine(Environment.NewLine + "Please choose from the list:" + Environment.NewLine);
            else
                Console.WriteLine("Have a nice day 😐");

            Console.WriteLine();
            await ChooseAsync(++tries, cancellationtoken);
            break;
    }

    // THis should be handled a better way to avoid StackOverflow.com
    return await ChooseAsync(tries, cancellationtoken);
}

async Task AuthenticateAndWelcomeAsync(CancellationToken cancellationtoken)
{
    var authentication = host.Services.GetService<Authentication>();
    var welcome = host.Services.GetService<Welcome>();

    var result = await authentication!.LogInAsync();
    var roll = welcome!.RollAsync(result!);
    await roll;
}

string DetermineEnvironment(int tries = 0)
{
    Console.WriteLine("In which environment will we be working?" + Environment.NewLine);
    Console.WriteLine("a - Development" + Environment.NewLine);
    Console.WriteLine("b - Staging" + Environment.NewLine);
    Console.WriteLine("c - Production" + Environment.NewLine);
    Console.WriteLine("x - Exit" + Environment.NewLine);

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "a": return "Development";
        case "b": return "Staging";
        case "c": return "Production";
        default:
            Console.WriteLine("Let's choose one of the above, okay?");
            if (tries > 3) Console.WriteLine("Have a nice day 😐");
            return DetermineEnvironment(++tries);
    }
}

void AddServices(HostApplicationBuilder builder, string environment, IConfigurationRoot configuration)
{
    //builder.Logging.ClearProviders();
    builder.Logging.AddSunAutoLogging();

    services.AddLogging();
    services.AddSingleton<Authentication>();
    services.AddSingleton<IBanner, BairesDev>();
    services.AddSingleton<Welcome>();

    services.AddScoped<LogGenerator>();
    services.AddScoped<LogUtilities>();

    var tableclienturi = environment switch
    {
        "Development" => new Uri(configuration["TableSasDev"]!),
        "Staging" => new Uri(configuration["TableSasStage"]!),
        "Production" => new Uri(configuration["TableSasProd"]!),
        _ => throw new ArgumentException("Variable out of range.", nameof(environment)),
    };

    services.AddScoped(options => new TableClient(tableclienturi));
}
