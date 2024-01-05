using Microsoft.Extensions.Configuration;

namespace SunAuto.UniversalLogging;

public class LoggerConfiguration(IConfiguration configuration)
{
    public string LevelDefault { get; private set; } = configuration[""]!.ToString();
}