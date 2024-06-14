using System.Net;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Api;

public class LoggingApi
{
    [Function("HttpTrigger1")]
    public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "products/{category:alpha}/{id:int?}")] HttpRequestData req, string category, int? id,
    FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("HttpTrigger1");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var message = String.Format($"Category: {category}, ID: {id}");
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");


        var content = JsonSerializer.Serialize(new { Message = message });
        response.WriteString(content);

        return response;
    }
}
