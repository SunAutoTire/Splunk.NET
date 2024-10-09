using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SunAuto.Logging.Client.TableStorage;

public class Storage : IStorage
{
    readonly HttpClient Client = new();
    readonly List<Task> UploadTasks = [];
    readonly string Application;
    readonly string ApiKey;
    //readonly JsonSerializerOptions JsonSerializerOptions;
    readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerOptions.Default);

    readonly List<QueueEntry> Queue = [];

    public Storage(IConfiguration configuration)
    {
        Application = configuration.GetSection("Logging:SunAuto")["Application"]!.ToString();
        ApiKey = configuration.GetSection("Logging:SunAuto")["ApiKey"]!.ToString();
        var baseurl = configuration.GetSection("Logging:SunAuto")["BaseUrl"]!.ToString();

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
            // We must handle this in CAR-403 ticket 

            //logger.LogCritical(9, new Exception("Exceptional!", new Exception("The Inner Light")), "Exceptions {Maybe} or {Possibly}?", "Maybe not", "Possibly");

        }
    }

    // public void Push<TState>(QueueEntry<TState> entry)
    // {
    //     Queue.Add(entry);

    //     if (Queue.Count > 9)
    //     {
    //         var items = Queue.ToArray();
    //         Queue.RemoveRange(0, 9);

    //         UploadTasks.Add(UploadAsync(items));
    //     }
    // }

    async Task UploadAsync(QueueEntry[] items)
    {
        try
        {
            var entries = Queue
                .ToArray()
                .Select(i =>
                {
                    var serializedex = JsonConvert.SerializeObject(i.Exception);

                    return new Entry
                    {
                        Application = Application,
                        Body = serializedex,
                        Level = i.Loglevel.ToString(),
                        Message = i.Formatted,
                    };
                });

            var serialized = JsonConvert.SerializeObject(items);
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


    //public Storage(IConfigurationSection configurationSection)
    //{

    //    Application = configurationSection["Application"]!.ToString();
    //    ApiKey = configurationSection["ApiKey"]!.ToString();


    //    try
    //    {
    //        Client = new HttpClient
    //        {
    //            BaseAddress = new Uri(baseurl),
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        System.Diagnostics.Debug.WriteLine(ex.Message);
    //        // We must handle this in CAR-403 ticket 

    //        //logger.LogCritical(9, new Exception("Exceptional!", new Exception("The Inner Light")), "Exceptions {Maybe} or {Possibly}?", "Maybe not", "Possibly");

    //    }

    //    JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerOptions.Default);
    //    JsonSerializerOptions.Converters.Add(new ExceptionConverter());
    //}

    public void Add<TState>(LogLevel logLevel, EventId eventId, TState? state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var entry = new QueueEntry
        {
            Loglevel = logLevel,
            EventId = eventId,
            State = state,
            Exception = exception,
            Formatted = formatter(state!, exception)
        };

        Queue.Add(entry);

        if (Queue.Count > 9)
        {
            var items = Queue.ToArray();
            Queue.RemoveRange(0, 9);

            UploadTasks.Add(UploadAsync(items));
        }

        //var formatted = formatter(state!, exception);

        //var serializedex = JsonSerializer.Serialize(exception, JsonSerializerOptions);

        //var entry = new Entry
        //{
        //    Application = Application,
        //    Body = serializedex,
        //    Level = logLevel.ToString(),
        //    Message = formatted,
        //};

        //var serialized = JsonSerializer.Serialize(entry);
        //var buffer = System.Text.Encoding.UTF8.GetBytes(serialized);
        //var byteContent = new ByteArrayContent(buffer);

        //byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //try
        //{
        //    Client.PostAsync($"api/{Application}/{logLevel}?code={ApiKey}", byteContent)
        //        .GetAwaiter()
        //        .GetResult();
        //}
        //catch (Exception ex)
        //{
        //    System.Diagnostics.Debug.WriteLine(ex.Message);
        //}
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
                var items = Queue.ToArray();
                Queue.Clear();

                UploadTasks.Add(UploadAsync(items));

                Task.Run(async () => await Task.WhenAll(UploadTasks));

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
