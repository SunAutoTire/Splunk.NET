using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client.TableStorage;

public class Storage(EntryStack stack) :
    IStorage, IDisposable
{
    //readonly HttpClient Client = null!;
    //readonly string Application= configurationSection["Application"]!.ToString();
    //readonly string ApiKey;
    //readonly JsonSerializerOptions JsonSerializerOptions;
    bool disposedValue;
    readonly EntryStack Stack = stack;

    //public Storage(IConfigurationSection configurationSection)
    //{

    //    Application = configurationSection["Application"]!.ToString();
    //    ApiKey = configurationSection["ApiKey"]!.ToString();

    //    var baseurl = configurationSection["BaseUrl"]!.ToString();

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
        Stack.Push(new QueueEntry<object>
        {
            Loglevel = logLevel,
            EventId = eventId,
            State = state,
            Exception = exception,
            Formatted = formatter(state!, exception)
        });

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



    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                //Client?.Dispose();
                Stack?.Dispose();
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
