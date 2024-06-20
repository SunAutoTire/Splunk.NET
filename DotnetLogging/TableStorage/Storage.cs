using Microsoft.Extensions.Logging;
using SunAuto.Logging.Client.FileStorage;
using SunAuto.Logging.Common;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SunAuto.Logging.Client.TableStorage;

public class Storage : IStorage
{
    readonly HttpClient Client;
    private readonly string Application = "LoggingTestConsole";

    public Storage(string environment)
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:7235"),

        };
    }

    public void Add<TState>(LogLevel logLevel, EventId eventId, TState? state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var formatted = formatter(state!, exception);

        var entry = new Entry
        {
            Application = Application,
            Body = exception,
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
