using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SunAuto.Logging;

public class Logger(IConfiguration configuration) :
    ILogger
{
     const string Name = "SunAuto";
    readonly LogLevel DefaultLevel = configuration.GetValue<string>("Logging:SunAuto:LogLevel:Default").ToLogLevel();
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();

    public bool IsEnabled(LogLevel logLevel) => logLevel >= DefaultLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            var originalColor = Console.ForegroundColor;

            try
            {
                var color = logLevel switch
                {
                    LogLevel.Trace => ConsoleColor.Cyan,
                    LogLevel.Debug => ConsoleColor.Green,
                    LogLevel.Information => ConsoleColor.White,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    LogLevel.Error => ConsoleColor.DarkRed,
                    LogLevel.Critical => ConsoleColor.Red,
                    //LogLevel.None
                    _ => ConsoleColor.Gray
                };

                Console.ForegroundColor = color;
                Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

                Console.ForegroundColor = originalColor;
                Console.Write($"     {Name} - ");

                Console.ForegroundColor = color;
                Console.Write($"{formatter(state, exception)}");
            }
            finally
            {
                Console.ForegroundColor = originalColor;
                Console.WriteLine();
            }
        }
    }
}
