using System;
using System.Net.Http;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ping
{
    public class Carify
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _requestUrl;

        public Carify(ILoggerFactory loggerFactory, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<Carify>();
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _requestUrl = configuration["RequestUrl"] ?? throw new ArgumentNullException("RequestUrl is missing in configuration");
        }

        [Function("Carify")]
        public async Task RunAsync([TimerTrigger("0 0 7-21 * * 1-5", RunOnStartup = true)] TimerInfo myTimer, CancellationToken cancellationToken)
        {
            var currentTime = DateTime.UtcNow;

            _logger.LogInformation("CarifyHourlyPing function starting at: {now}", currentTime);

            try
            {
                var response = await _httpClient.GetAsync(_requestUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Ping to {url} failed with status code {statusCode} at: {now}", _requestUrl, response.StatusCode, currentTime);
                    await NotifyIfDownAsync(response.StatusCode, currentTime, _logger);
                }
                else
                {
                    _logger.LogInformation("Ping successful to {url} at: {now}", _requestUrl, currentTime);
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
            await Task.CompletedTask;
        }
    }
}
