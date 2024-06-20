using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunAuto.Extensions;

namespace SunAuto.Logging.Client;

public class Logger(IConfiguration configuration) : ILogger
{
    private readonly IConfiguration Configuration = configuration;
    private IStorage? Storage;

    IStorage GetStorage()
    {
        if (Storage == null)
        {
            var environment = Environment.GetEnvironmentVariable("LoggingEnvironment");
            var x = Environment.GetEnvironmentVariables();

            Storage = environment switch
            {
                "Development" or "Staging" or "Test" or "Production" => new TableStorage.Storage(environment),
                _ => new FileStorage.Storage(Configuration.GetValue<string>("Logging:SunAuto:Path")!),
            };
        }

        return Storage;
    }

    readonly LogLevel DefaultLevel = GetLogLevel(configuration);

    private static LogLevel GetLogLevel(IConfiguration configuration)
    {
        try
        {
            return configuration.GetValue<string>("Logging:SunAuto:LogLevel:Default").ToLogLevel();
        }
        catch (ArgumentException ex)
        {
            var message = typeof(Logger).GetEmbeddedResource("SunAuto.Logging.ConfigurationExMessage.txt");
            throw new InvalidOperationException(message, ex);
        }
    }

    //private static string GetMessage()
    //{
    //    var output = new StringBuilder();

    //    output.AppendLine("SunAuto.Logging requires the following JSON to be added to the \"Logging\" object in the appsettings.json");
    //    output.AppendLine("e.g.,");
    //    output.AppendLine();

    //    output.AppendLine(" \"SunAuto\": {");
    //    output.AppendLine("   \"LogLevel\": {");
    //    output.AppendLine("     \"Default\": \"Trace\",");
    //    output.AppendLine("     \"Microsoft.Hosting\": \"Trace\"");
    //    output.AppendLine("   }");
    //    output.AppendLine(" },");
    //    output.AppendLine();

    //    return output.ToString();
    //}

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= DefaultLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
            GetStorage().Add(logLevel, eventId, state, exception, formatter);
    }
}
