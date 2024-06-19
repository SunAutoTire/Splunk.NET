
// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SunAuto.Development.Console;
using SunAuto.Logging.Client;
using SunAuto.Logging.Client.FileStorage;

var _ = new Welcome(roll: true);

Environment.SetEnvironmentVariable("LoggingEnvironment", "Local");

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddLogging();

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogDebug(1, "Does this line get hit?");    // Not logged
logger.LogInformation(3, "Nothing to see here."); // Logs in ConsoleColor.DarkGreen
logger.LogWarning(5, "Warning... that was odd."); // Logs in ConsoleColor.DarkCyan
logger.LogError(7, "Oops, there was an error.");  // Logs in ConsoleColor.DarkRed
logger.LogTrace(5, "== 120.");                    // Not logged
logger.LogCritical(9, new Exception("Exceptional!", new Exception("The Inner Light")), "Exceptions {Maybe}?", "Maybe not");
logger.LogCritical(9, new Exception("Exceptional!", new Exception("The Inner Light")), "Exceptions {Maybe} or {Possibly}?", "Maybe not", "Possibly");

var storage = new Storage("..\\..\\..\\SunAuto.log");

var check = storage.List();

foreach (var item in check)
    Console.WriteLine(item.TimeStamp);

await host.RunAsync();
