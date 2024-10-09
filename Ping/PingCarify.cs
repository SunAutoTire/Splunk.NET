using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ping
{
    public class PingCarify
    {
        private readonly ILogger<PingCarify> _logger;
        private readonly HttpClient _httpClient;

        public PingCarify(ILogger<PingCarify> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [Function("CarifyHourlyPing")]
        public async Task RunAsync([Microsoft.Azure.Functions.Worker.TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo myTimer, CancellationToken cancellationToken)
        {
            var currentTime = DateTime.UtcNow.AddHours(-5);
            if (currentTime.Hour < 7 || currentTime.Hour > 21)
            {
                _logger.LogInformation("Outside of business hours. No ping performed.");
                return;
            }

            _logger.LogInformation("CarifyHourlyPing function starting at: {now}", currentTime);

            try
            {
                var requestUrl = "https://sunautocarifyn.azurewebsites.net/";
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Ping to {url} failed with status code {statusCode} at: {now}", requestUrl, response.StatusCode, currentTime);
                    // Notify only if there’s an issue
                    await NotifyIfDownAsync(response.StatusCode, currentTime, _logger);
                }
                else
                {
                    _logger.LogInformation("Ping successful to {url} at: {now}", requestUrl, currentTime);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unexpected error: {message}", ex.Message);
                await NotifyIfDownAsync(null, currentTime, _logger);
            }

            if (myTimer.ScheduleStatus is not null)
                _logger.LogInformation("Next CarifyHourlyPing at: {next}", myTimer.ScheduleStatus.Next);
        }

        private async Task NotifyIfDownAsync(HttpStatusCode? statusCode, DateTime currentTime, ILogger log)
        {
            var notificationMessage = $"Carify application is down at {currentTime}. Status code: {statusCode?.ToString() ?? "No Response"}";
            log.LogCritical(notificationMessage);
            // Implement the notification logic (e.g., send an email or SMS)
            await Task.CompletedTask; // Placeholder for actual notification implementation
        }
    }
}
