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
    private bool _flushing;
    private bool _disposed;
    readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new ExceptionConverter()
        }
    };

    /// <summary>
    /// Initializes a new instance of the SplunkSink class with the specified base URL, token, and source. This constructor sets up the HTTP client for sending log entries to the Splunk HTTP Event Collector (HEC) endpoint, including configuring the authorization header with the provided token.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Splunk HTTP Event Collector (HEC) endpoint.</param>
    /// <param name="token">The authentication token for the Splunk HEC.</param>
    /// <param name="source">The source identifier for the log entries.</param>
    internal SplunkSink(string baseUrl, string token, string source)
    {
        _source = source;
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", token);
    }

    /// <summary>
    /// Writes a log entry to the Splunk sink. This method adds the provided QueueEntry to the internal queue and initiates an asynchronous flush operation to send the queued entries to Splunk. If the sink has been disposed, the method will return without performing any action.
    /// </summary>
    /// <param name="line">The log entry to be written to the Splunk sink.</param>
    internal void Write(QueueEntry line)
    {
        lock (_lock)
        {
            if (_disposed) return;

            _queue.Add(line);

            if (_flushing) return;

            _flushing = true;
            _handler = FlushAsync();
        }
    }

    /// <summary>
    /// Drains the queue until it is empty. <see cref="_flushing"/> is cleared under the same
    /// lock that observes the empty queue, so a <see cref="Write"/> racing this method either
    /// sees the pump still running (and its entry is picked up by the next iteration) or sees
    /// it stopped (and starts a new one). Keying off the task's completion state instead would
    /// leave a window where neither happens and the entry is stranded.
    /// </summary>
    private async Task FlushAsync()
    {
        try
        {
            while (true)
            {
                QueueEntry[] batch;

                lock (_lock)
                {
                    if (_queue.Count == 0)
                    {
                        _flushing = false;
                        return;
                    }

                    batch = _queue.ToArray();
                    _queue.Clear();
                }

                await PostAsync(batch);
            }
        }
        catch
        {
            // PostAsync reports its own failures, so anything reaching here is unexpected.
            // Clear the flag regardless, otherwise no later Write could restart the pump.
            lock (_lock) _flushing = false;
            throw;
        }
    }

    private async Task PostAsync(QueueEntry[] items)
    {
        try
        {
            var payload = string.Concat(items.Select(i =>
            {
                var serializedex = JsonSerializer.Serialize(i.Exception, JsonSerializerOptions);

                using var doc = JsonDocument.Parse(serializedex);

                var entry = new Entry
                {
                    @Event = new Event
                    {
                        Body = doc.RootElement.Clone(),
                        Level = i.Loglevel.ToString(),
                        Message = i.Formatted!,
                        Timestamp = i.Timestamp,
                        EventId = i.EventId.Id,
                        UserId = null,
                        EventName = i.EventId.Name
                    },
                    SourceType = _source
                };

                return JsonSerializer.Serialize(entry) + "\n";
            }));

            var json = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("services/collector/event", json);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                ReportError($"Splunk HEC returned {(int)response.StatusCode}: {content}");
        }
        catch (Exception ex)
        {
            ReportError($"Splunk HEC post failed: {ex}");
        }
    }

    private static void ReportError(string message)
    {
        Console.Error.WriteLine($"[SunAuto.Logging] {message}");
        // OnError?.Invoke(message);   // optional hook, e.g. surfaced via LoggerOptions
    }

    /// <summary>
    /// Disposes the SplunkSink instance, ensuring that any remaining log entries in the queue are flushed to the Splunk HTTP Event Collector (HEC) endpoint before the sink is disposed. This method is thread-safe and can be called multiple times without throwing exceptions. It waits for any ongoing flush operations to complete before disposing of the HTTP client.
    /// </summary>    
    public void Dispose()
    {
        Task pending;

        lock (_lock)
        {
            if (_disposed) return;

            // Set first so no further entries are accepted while we drain.
            _disposed = true;
            pending = _handler;
        }

        try
        {
            pending.GetAwaiter().GetResult();

            // The pump stops on an empty queue, but a Write may have landed after that
            // observation and before _disposed was set. Post whatever is left.
            QueueEntry[] remaining;

            lock (_lock)
            {
                remaining = _queue.ToArray();
                _queue.Clear();
            }

            if (remaining.Length > 0)
                PostAsync(remaining).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            ReportError($"Splunk HEC flush on dispose failed: {ex}");
        }

        _client.Dispose();
    }
}
