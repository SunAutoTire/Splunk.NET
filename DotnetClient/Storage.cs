using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SunAuto.Splunk.Client;

/// <summary>
/// Sends log entries to Splunk via the HTTP Event Collector (HEC).
/// Entries are queued synchronously and flushed asynchronously in batches.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of <see cref="Storage"/>, reading Splunk connection
    /// settings from the specified configuration section.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="sectionName">
    /// The configuration section path containing <c>Source</c>, <c>Token</c>, and <c>BaseUrl</c>.
    /// Defaults to <c>Logging:SunAuto</c>.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>Source</c>, <c>Token</c>, or <c>BaseUrl</c> are missing from the configuration section.
    /// </exception>
    public Storage(IConfiguration configuration, string sectionName = "Logging:SunAuto")
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = loggerFactory.CreateLogger<Storage>();

        var section = configuration.GetSection(sectionName);

        Source = section["Source"]?.ToString()
            ?? throw new InvalidOperationException($"The \"Source\" property is required in the {sectionName} configuration section.");
        Token = section["Token"]?.ToString()
            ?? throw new InvalidOperationException($"The \"Token\" property is required in the {sectionName} configuration section.");
        var baseurl = section["BaseUrl"]?.ToString()
            ?? throw new InvalidOperationException($"The \"BaseUrl\" property is required in the {sectionName} configuration section.");

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
            Logger.LogCritical(ex, "Storage initialization failed. Check configuration for Splunk logging.");
        }
    }

    /// <summary>
    /// Drains the queue by uploading all pending entries in batches until the queue is empty.
    /// </summary>
    async Task HandleQueueAsync()
    {
        while (Queue.Count > 0)
        {
            var items = Queue.ToArray();
            Queue.RemoveRange(0, Queue.Count);

            await UploadAsync(items);
        }
    }

    /// <summary>
    /// Serializes the given queue entries and POSTs them to the Splunk HEC endpoint.
    /// </summary>
    /// <param name="items">The entries to upload.</param>
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

    /// <inheritdoc/>
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

    /// <summary>
    /// Releases managed resources. Blocks until the upload queue is fully drained.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Handler.GetAwaiter().GetResult();

                Client?.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
