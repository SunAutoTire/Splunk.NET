using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SunAuto.Logging;

internal sealed class SplunkSink : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _source;
    private readonly List<string> _queue = [];
    private Task _handler = Task.CompletedTask;
    private readonly object _lock = new();
    private bool _disposed;

    internal SplunkSink(string baseUrl, string token, string source)
    {
        _source = source;
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", token);
    }

    internal void Write(string line)
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
            string[] batch;
            lock (_lock)
            {
                if (_queue.Count == 0) break;
                batch = [.. _queue];
                _queue.Clear();
            }
            await PostAsync(batch);
        }
    }

    private async Task PostAsync(string[] lines)
    {
        try
        {
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.Append(JsonSerializer.Serialize(new { @event = line, sourcetype = _source }));
                sb.Append('\n');
            }

            var content = new StringContent(sb.ToString(), Encoding.UTF8, "application/json");
            await _client.PostAsync("services/collector/event", content);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SplunkSink post failed: {ex.Message}");
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
