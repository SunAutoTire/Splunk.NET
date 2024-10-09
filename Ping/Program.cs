using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        var connectionstring = Environment.GetEnvironmentVariable("ConnectionString")!;
        services.AddScoped(options => new BlobServiceClient(new Uri(connectionstring)));
    })
    .Build();

host.Run();
