using Microsoft.Extensions.Configuration;

namespace SunAuto.Logging.Client;

public class LoggerConfiguration(IConfiguration configuration)
{
    public string LevelDefault { get; private set; } = configuration[""]!.ToString();
}