using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Api
{
    public class Function
    {
        private readonly ILogger<Function> _logger;

        public Function(ILogger<Function> logger)
        {
            _logger = logger;
        }

        [Function("Function")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            try
            {
                var environmentvariables = Environment.GetEnvironmentVariables();
                var connectionstring = environmentvariables["ConnectionStrings:UniversalLoggingConnectionString"]!.ToString();
                // var connectionstring = environmentvariables["UniversalLoggingConnectionString"]!.ToString();
                var environment = environmentvariables["AZURE_FUNCTIONS_ENVIRONMENT"]!.ToString();

                var output = String.Format("C# HTTP trigger function processed a request. {0} {1}", connectionstring, environment);
                _logger.LogInformation(output);
                return new OkObjectResult(output);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.StackTrace);
            }
        }
    }
}
