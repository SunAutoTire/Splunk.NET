using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()  // Correct for isolated worker
    .ConfigureServices(services =>
    {
        // Adding HttpClient and Application Insights telemetry for the isolated worker
        services.AddApplicationInsightsTelemetryWorkerService();
        services.AddHttpClient();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();  // Optional: Adds console logging for local debugging
    })
    .Build();

host.Run();
