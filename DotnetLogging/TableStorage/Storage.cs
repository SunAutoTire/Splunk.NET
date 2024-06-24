using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunAuto.Logging.Client.FileStorage;
using SunAuto.Logging.Common;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace SunAuto.Logging.Client.TableStorage;

public class Storage : IStorage
{
    readonly HttpClient Client;
    readonly string Application = "LoggingTestConsole";
    readonly string ApiKey;
    readonly JsonSerializerOptions JsonSerializerOptions;

    public Storage(IConfigurationSection configurationSection)
    {
        Application = configurationSection["Application"]!.ToString();
        var environment = configurationSection["ApiKey"]!.ToString();
        ApiKey = configurationSection["ApiKey"]!.ToString();

        var vars = Environment.GetEnvironmentVariables();

        foreach (System.Collections.DictionaryEntry item in vars)
            System.Diagnostics.Debug.WriteLine($"{item.Key} {item.Value}");

        var baseurl = configurationSection["BaseUrl"]!.ToString();

        Client = new HttpClient
        {
            BaseAddress = new Uri(baseurl),
        };

        JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerOptions.Default);
        JsonSerializerOptions.Converters.Add(new ExceptionConverter());
    }

    public void Add<TState>(LogLevel logLevel, EventId eventId, TState? state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var formatted = formatter(state!, exception);

        var serializedex = JsonSerializer.Serialize(exception, JsonSerializerOptions);

        var entry = new Entry
        {
            Application = Application,
            Body = serializedex,
            Level = logLevel.ToString(),
            Message = formatted,
        };

        var serialized = JsonSerializer.Serialize(entry);
        var buffer = System.Text.Encoding.UTF8.GetBytes(serialized);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var result = Client.PostAsync($"api/{Application}/{logLevel}", byteContent)
            .GetAwaiter()
            .GetResult();

        //throw new NotImplementedException();
    }

    public void Delete(EventId eventId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<LogItem> List()
    {
        throw new NotImplementedException();
    }
}
