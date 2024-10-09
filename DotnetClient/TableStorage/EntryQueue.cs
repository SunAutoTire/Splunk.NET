// using Microsoft.Extensions.Configuration;
// using System.Net.Http.Headers;
// using System.Text;
// using System.Text.Json;

// namespace SunAuto.Logging.Client.TableStorage;

// public class EntryQueue : IDisposable
// {
//     readonly HttpClient Client = new();


//     readonly string Application = null!;
//     readonly string ApiKey = null!;

//     public EntryQueue(IConfiguration configuration)
//     {
//         Application = configuration.GetSection("Logging:SunAuto")["Application"]!.ToString();
//         ApiKey = configuration.GetSection("Logging:SunAuto")["ApiKey"]!.ToString();
//         Client.BaseAddress = new Uri(configuration.GetSection("Logging:SunAuto")["BaseUrl"]!.ToString());
//     }
//     #region IDisposable

//     protected virtual void Dispose(bool disposing)
//     {
//         if (!disposedValue)
//         {
//             if (disposing)
//             {
//                 Task.Run(async () => await Task.WhenAll(UploadTasks));

//                 Client?.Dispose();
//             }

//             // TODO: free unmanaged resources (unmanaged objects) and override finalizer
//             // TODO: set large fields to null
//             disposedValue = true;
//         }
//     }

//     // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
//     // ~EntryStack()
//     // {
//     //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//     //     Dispose(disposing: false);
//     // }

//     public void Dispose()
//     {
//         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//         Dispose(disposing: true);
//         GC.SuppressFinalize(this);
//     }

//     #endregion
// }
