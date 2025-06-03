using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SunAuto.Logging.Client.Splunk;

public class Storage : IStorage
{
    readonly HttpClient Client = new();
    Task Handler = Task.CompletedTask;
    readonly string Token;
    readonly string Source;
    readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new ExceptionConverter()
        }
    };

    readonly List<QueueEntry> Queue = [];
    private readonly ILogger<Storage> Logger;

    public Storage(IConfiguration configuration, string sectionName = "Logging:SunAuto")
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        Logger = loggerFactory.CreateLogger<Storage>();
        Logger.LogInformation("Hello World! Logging is {Description}.", "fun");
      
        
        var section = configuration.GetSection(sectionName);

        Source = section["Source"]!.ToString();
        Token = section["Token"]!.ToString();
        var baseurl = section["BaseUrl"]!.ToString();

        try
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(baseurl),
            };
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", Token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
            //Logger.LogCritical(ex, "Storage initialization failed. Check configuration for Splunk logging.");
            // We must handle this in CAR-403 ticket 

            //logger.LogCritical(9, new Exception("Exceptional!", new Exception("The Inner Light")), "Exceptions {Maybe} or {Possibly}?", "Maybe not", "Possibly");
        }
    }

    async Task HandleQueueAsync()
    {
        while (Queue.Count > 0)
        {
            var items = Queue.ToArray();
            Queue.RemoveRange(0, Queue.Count);

            await UploadAsync(items);
        }
    }

    async Task UploadAsync(QueueEntry[] items)
    {
        try
        {
            var entries = items
                .Select(i =>
                {
                    var serializedex = JsonSerializer.Serialize(i.Exception, JsonSerializerOptions);

                    using var doc = JsonDocument.Parse(serializedex);
                    var bodyElement = doc.RootElement.Clone();

                    return new Entry
                    {
                        @event = new Event
                        {
                            Body = bodyElement,
                            Level = i.Loglevel.ToString(),
                            Message = i.Formatted!,
                            Timestamp = i.Timestamp,
                            EventId = i.EventId.Id,
                            UserId = null,
                            EventName = i.EventId.Name
                        },
                        sourcetype = Source
                    };
                });

            var serialized = JsonSerializer.Serialize(entries);
            var json = new StringContent(serialized, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("services/collector/event", json);

            var content = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    public void Add<TState>(LogLevel logLevel, EventId eventId, TState? state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var entry = new QueueEntry
        {
            Loglevel = logLevel,
            EventId = eventId,
            State = state,
            Exception = exception,
            Formatted = formatter(state!, exception),
        };

        Queue.Add(entry);

        if (Handler.IsCanceled || Handler.IsFaulted || Handler.IsCompleted)
            Handler = HandleQueueAsync();
    }

    bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Handler.GetAwaiter().GetResult();

                Client?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Storage()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
