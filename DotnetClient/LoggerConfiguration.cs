using Microsoft.Extensions.Configuration;

namespace SunAuto.Splunk.Client;

public class LoggerConfiguration(IConfiguration configuration)
{
    public string LevelDefault { get; set; } = configuration["Logging:SunAuto:LogLevel:Default"]!.ToString();
}