using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace SunAuto.Logging.Client.Test;

/// <summary>
/// A stand-in Splunk HTTP Event Collector. Captures the exact bytes the sink puts on the wire so
/// tests can assert on payload shape rather than on internal state.
/// </summary>
/// <remarks>
/// Each request body is recorded <em>before</em> the response is sent, so once the sink's POST has
/// completed the body is already visible to the test. That keeps the tests deterministic without
/// polling or arbitrary delays.
/// </remarks>
internal sealed class FakeHec : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly List<string> _bodies = [];
    private readonly List<string> _authorization = [];
    private readonly List<string> _paths = [];
    private readonly Task _pump;
    private readonly HttpStatusCode _status;

    public FakeHec(HttpStatusCode status = HttpStatusCode.OK)
    {
        _status = status;
        BaseUrl = $"http://127.0.0.1:{FreePort()}/";
        _listener.Prefixes.Add(BaseUrl);
        _listener.Start();
        _pump = Task.Run(PumpAsync);
    }

    /// <summary>Base URL to hand to <see cref="LoggerOptions.SplunkOptions.BaseUrl"/>.</summary>
    public string BaseUrl { get; }

    /// <summary>Raw request bodies, one per POST.</summary>
    public IReadOnlyList<string> Bodies { get { lock (_bodies) return [.. _bodies]; } }

    /// <summary>Every non-empty line across all request bodies. One line per event when the payload is well formed.</summary>
    public IReadOnlyList<string> Lines =>
        [.. Bodies.SelectMany(b => b.Split('\n', StringSplitOptions.RemoveEmptyEntries))];

    /// <summary>Authorization header value of each POST.</summary>
    public IReadOnlyList<string> Authorization { get { lock (_bodies) return [.. _authorization]; } }

    /// <summary>Absolute path of each POST.</summary>
    public IReadOnlyList<string> Paths { get { lock (_bodies) return [.. _paths]; } }

    public int RequestCount => Bodies.Count;

    /// <summary>Parses every recorded line, failing the test if any line is not standalone JSON.</summary>
    public IReadOnlyList<JsonElement> ParsedEvents()
    {
        var parsed = new List<JsonElement>();

        foreach (var line in Lines)
        {
            try
            {
                using var doc = JsonDocument.Parse(line);
                parsed.Add(doc.RootElement.Clone());
            }
            catch (JsonException ex)
            {
                Assert.Fail($"Posted line is not valid standalone JSON: {ex.Message}\nLine: {line}");
            }
        }

        return parsed;
    }

    private async Task PumpAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (HttpListenerException)
            {
                break;      // listener stopped
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                var body = await reader.ReadToEndAsync();

                lock (_bodies)
                {
                    _bodies.Add(body);
                    _authorization.Add(context.Request.Headers["Authorization"] ?? string.Empty);
                    _paths.Add(context.Request.Url?.AbsolutePath ?? string.Empty);
                }
            }

            var payload = Encoding.UTF8.GetBytes(_status == HttpStatusCode.OK
                ? """{"text":"Success","code":0}"""
                : """{"text":"Incorrect index","code":7}""");

            context.Response.StatusCode = (int)_status;
            context.Response.ContentType = "application/json";
            await context.Response.OutputStream.WriteAsync(payload);
            context.Response.Close();
        }
    }

    private static int FreePort()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        return port;
    }

    public void Dispose()
    {
        if (_listener.IsListening)
            _listener.Stop();

        _pump.Wait(TimeSpan.FromSeconds(5));
        _listener.Close();
    }
}
