using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using SunAuto.Logging.Common;

namespace SunAuto.Logging.Client.TableStorage;

public class Storage : IStorage
{
    readonly HttpClient Client;
    readonly List<Task> UploadTasks = new();
    readonly string Application;
    readonly string ApiKey;
    readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new ExceptionConverter()
        }
    };

    readonly List<QueueEntry> PreQueue = new();
    readonly List<QueueEntry> Queue = new();
    private readonly TimeSpan FlushInterval;
    private readonly int MaxPreQueueSize;

    public Storage(IConfiguration configuration)
    {
        Application = configuration.GetSection("Logging:SunAuto")["Application"]!.ToString();
        ApiKey = configuration.GetSection("Logging:SunAuto")["ApiKey"]!.ToString();
        var baseurl = configuration.GetSection("Logging:SunAuto")["BaseUrl"]!.ToString();

        int flushIntervalSeconds = int.Parse(configuration.GetSection("Logging:SunAuto")["FlushInterval"] ?? "10");
        FlushInterval = TimeSpan.FromSeconds(flushIntervalSeconds);
        MaxPreQueueSize = int.Parse(configuration.GetSection("Logging:SunAuto")["MaxPreQueueSize"] ?? "100");

        try
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(baseurl),
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

        StartPeriodicFlush();
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

        PreQueue.Add(entry);

        if (PreQueue.Count >= MaxPreQueueSize)
        {
            FlushPreQueueToMainQueue();
        }
    }

    private void FlushPreQueueToMainQueue()
    {
        Queue.AddRange(PreQueue);
        PreQueue.Clear();
        HandleQueue();
    }

    private void StartPeriodicFlush()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(FlushInterval);
                FlushPreQueueToMainQueue();
            }
        });
    }

    void HandleQueue(bool handleAll = false)
    {
        while (Queue.Count > 9 || handleAll)
        {
            var items = Queue.ToArray();
            Queue.Clear();
            UploadTasks.Add(UploadAsync(items));
        }
    }

    async Task UploadAsync(QueueEntry[] items)
    {
        try
        {
            var entries = items.Select(i =>
            {
                var serializedex = JsonSerializer.Serialize(i.Exception, JsonSerializerOptions);

                return new EntryUpdateRequest
                {
                    Application = Application,
                    Body = serializedex,
                    Level = i.Loglevel.ToString(),
                    Message = i.Formatted,
                    Timestamp = i.Timestamp,
                    EventId = i.EventId.Id,
                    EventName = i.EventId.Name
                };
            });

            var serialized = JsonSerializer.Serialize(entries, JsonSerializerOptions);
            var buffer = Encoding.UTF8.GetBytes(serialized);
            var byteContent = new ByteArrayContent(buffer);

            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            await Client.PostAsync($"api?code={ApiKey}", byteContent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    public void Delete(EventId eventId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<LogItem> List()
    {
        throw new NotImplementedException();
    }

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                FlushPreQueueToMainQueue();
                HandleQueue(true);

                Task.WhenAll(UploadTasks).GetAwaiter().GetResult();
                Client?.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
