using Microsoft.Extensions.Configuration;
using SunAuto.Logging.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SunAuto.Logging.Client.TableStorage;

public class EntryStack : IDisposable
{
    readonly HttpClient Client = new();

    readonly Queue<QueueEntry<object>> Queue = new();
    Task? Runner;
    private bool disposedValue;
    readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerOptions.Default);
    readonly string Application = null!;
    readonly string ApiKey = null!;

    public EntryStack(IConfiguration configuration)
    {
        Application = configuration.GetSection("Logging:SunAuto")["Application"]!.ToString();
        ApiKey = configuration.GetSection("Logging:SunAuto")["ApiKey"]!.ToString();
        Client.BaseAddress = new Uri(configuration["TableSas"]!);
    }

    public async void Push(QueueEntry<object> queueEntry)
    {
        Queue.Enqueue(queueEntry);

        if (Runner is null || Runner?.Status == TaskStatus.RanToCompletion || Runner?.Status == TaskStatus.Faulted)
            Runner = RunAsync();

        await Task.CompletedTask;
    }

    async Task RunAsync()
    {
        var count = Queue.Count;
        var bodycount = 0;

        while (count > 0)
        {
            try
            {
                var items = Queue
                    .ToArray()
                    .Select(i =>
                    {
                        var serializedex = JsonSerializer.Serialize(i.Exception, JsonSerializerOptions);

                        return new Entry
                        {
                            Application = Application,
                            Body = serializedex,
                            Level = i.Loglevel.ToString(),
                            Message = i.Formatted,
                        };
                    });

                bodycount = items.Count();
                var serialized = JsonSerializer.Serialize(items);
                var buffer = Encoding.UTF8.GetBytes(serialized);
                var byteContent = new ByteArrayContent(buffer);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                await Client.PostAsync($"api/{Application}?code={ApiKey}", byteContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                for (int i = 0; i < count; i++)
                    Queue.Dequeue();
            }

            count = Queue.Count;
        }

        Runner = null;
    }


    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                while (Queue.Count > 0)
                    Task.Delay(100).GetAwaiter().GetResult();

                Client?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~EntryStack()
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

    #endregion
}
