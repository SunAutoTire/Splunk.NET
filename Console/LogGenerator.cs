using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Console;

public class LogGenerator(ILoggerFactory loggerFactory)
{
    private readonly ILogger Logger = loggerFactory.CreateLogger<LogGenerator>();

    public string? UserId { get; set; }

    public async void Run()
    {
        var task1 = LogAsync();
        var task2 = LogAsync();
        var task3 = LogAsync();
        var task4 = LogAsync();
        var task5 = LogAsync();
        var task6 = LogAsync();
        var task7 = LogAsync();
        var task8 = LogAsync();

        await Task.WhenAll([task1, task2, task3, task4, task5, task6, task7, task8]);
    }

    public async Task LogAsync()
    {
        Logger.LogDebug(1, "Does this line get hit? [user={user}]", UserId);    // Not logged
        Logger.LogDebug(1, "Does this line get hit?");    // Not logged
        Logger.LogInformation(3, "Nothing to see here."); // Logs in ConsoleColor.DarkGreen
        Logger.LogWarning(5, "Warning... that was odd."); // Logs in ConsoleColor.DarkCyan
        Logger.LogError(7, "Oops, there was an error.");  // Logs in ConsoleColor.DarkRed
        Logger.LogTrace(5, "== 120.");                    // Not logged
        Logger.LogCritical(9, new Exception("Exceptional!", new Exception("The Inner Light")), "Exceptions {Maybe}?", "Maybe not");
        Logger.LogCritical(9, new Exception("Exceptional!", new Exception("The Inner Light")), "Exceptions {Maybe} or {Possibly}?", "Maybe not", "Possibly");

        object? nullobject = null;

        try
        {
            var check1 = nullobject!.ToString();
        }
        catch (NullReferenceException ex)
        {
            Logger.LogError(ex, "Null reference.");
        }

        // var storage = new Storage("..\\..\\..\\SunAuto.log");

        // var check = storage.List();

        // foreach (var item in check)
        //     Console.WriteLine(item.TimeStamp);

        await Task.CompletedTask;
    }
}
