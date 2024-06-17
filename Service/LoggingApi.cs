using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace SunAuto.Logging.Api;

public class LoggingApi
{
    [Function("LoggerItem")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", "delete", Route = "api/{application:alpha?}/{environment:alpha?}/{level:alpha?}")] HttpRequestData req,
                                string? application,
                                string? environment,
                                string? level,
                                FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("LoggerItem");
        logger.LogInformation("POST Log item {url}.", req.Url);
        try
        {
            switch (req.Method)
            {
                //case "GET":
                //    break;
                case "POST":
                    var body = req.Body;
                    Create(application, environment, level, body);
                    break;
                //case "DELETE":
                default:
                    throw new NotImplementedException();
            }

            var message = String.Format($"Category: {category}, ID: {id}");
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");


            var content = JsonSerializer.Serialize(new { Message = message });
            response.WriteString(content);

            return response;
        }
        catch (ArgumentException ex)
        {
            return logger.HandleError(req, ex, HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            return logger.HandleError(req, ex, HttpStatusCode.InternalServerError);
        }
    }

    void Create(string? application, string? environment, string? level, Stream body)
    {

        if (String.IsNullOrWhiteSpace(application)) throw new ArgumentException("Application must be set in the route.", nameof(application));
        if (String.IsNullOrWhiteSpace(environment)) throw new ArgumentException("Environment must be set in the route.", nameof(environment));
        if (String.IsNullOrWhiteSpace(level)) throw new ArgumentException("Level must be set in the route.", nameof(level));

        return req.CreateResponse(HttpStatusCode.NotFound);
        throw new NotImplementedException();
    }
}
