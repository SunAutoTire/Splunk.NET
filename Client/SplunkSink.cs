using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SunAuto.Logging.Client;

internal sealed class SplunkSink : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _source;
    private readonly List<QueueEntry> _queue = [];
    private Task _handler = Task.CompletedTask;
    private readonly object _lock = new();
    private bool _disposed;
    readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new ExceptionConverter()
        }
    };

    internal SplunkSink(string baseUrl, string token, string source)
    {
        _source = source;
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", token);
    }

    internal void Write(QueueEntry line)
    {
        lock (_lock)
        {
            if (_disposed) return;
            _queue.Add(line);
            if (_handler.IsCompleted || _handler.IsFaulted || _handler.IsCanceled)
                _handler = FlushAsync();
        }
    }

    private async Task FlushAsync()
    {
        while (true)
        {
            QueueEntry[] batch;
            lock (_lock)
            {
                if (_queue.Count == 0) break;
                batch = _queue.ToArray();
                _queue.Clear();
            }
            await PostAsync(batch);
        }
    }

    private async Task PostAsync(QueueEntry[] items)
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
                        sourcetype = _source
                    };
                });

            var serialized = JsonSerializer.Serialize(entries);
            var json = new StringContent(serialized, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("services/collector/event", json);

            var content = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;
        }

        _handler.GetAwaiter().GetResult();
        _client.Dispose();
    }
}
