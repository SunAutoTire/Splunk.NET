using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace SunAuto.Logging.Api;

public static class Extensions
{
    internal static HttpResponseData HandleError(this ILogger logger, HttpRequestData requestData, Exception ex, HttpStatusCode statusCode)
    {
        logger.LogIt(statusCode, ex);

        var response = requestData.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        var content = JsonSerializer.Serialize(new { ex.Message });
        response.WriteString(content);

        return response;
    }

    static void LogIt(this ILogger logger, HttpStatusCode statusCode, Exception ex)
    {
        switch (statusCode)
        {
            case HttpStatusCode.NotFound:
                logger.LogWarning(ex, "{message}", ex.Message);
                break;
            case HttpStatusCode.InternalServerError:
            default:
                logger.LogCritical(ex, "{message}", ex.Message);
                break;
        }
    }
}
