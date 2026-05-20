using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SunAuto.Splunk.Client.Test;

public class LoggerTest
{
    [Theory(DisplayName = "IsEnabled - Configuration")]
    [InlineData(LogLevel.Critical, LogLevel.Critical, true)]
    [InlineData(LogLevel.Debug, LogLevel.Debug, true)]
    [InlineData(LogLevel.Error, LogLevel.Error, true)]
    [InlineData(LogLevel.Information, LogLevel.Information, true)]
    [InlineData(LogLevel.Trace, LogLevel.Trace, true)]
    [InlineData(LogLevel.Warning, LogLevel.Warning, true)]
    [InlineData(LogLevel.Information, LogLevel.Trace, false)]
    public void Test0(LogLevel defaultLevel, LogLevel logLevel, bool expected)
    {
        var logger = GetLogger(defaultLevel);

        Assert.Equal(expected, logger.IsEnabled(logLevel));
    }

    [Fact(DisplayName = "IsEnabled - LogLevel.None is never enabled")]
    public void Test1()
    {
        foreach (LogLevel defaultLevel in Enum.GetValues<LogLevel>())
        {
            var logger = GetLogger(defaultLevel);

            Assert.False(logger.IsEnabled(LogLevel.None));
        }
    }

    [Fact(DisplayName = "IsEnabled - Falls back to Information when Default key is absent")]
    public void Test2()
    {
        var options = new SimpleOptionsMonitor(new LoggerConfiguration { LogLevel = [] });
        var logger = new Logger("TestCategory", options, null!);

        Assert.False(logger.IsEnabled(LogLevel.Debug));
        Assert.True(logger.IsEnabled(LogLevel.Information));
    }

    private static Logger GetLogger(LogLevel level) =>
        new("TestCategory", new SimpleOptionsMonitor(new LoggerConfiguration
        {
            LogLevel = new() { ["Default"] = level }
        }), null!);

    private sealed class SimpleOptionsMonitor(LoggerConfiguration config) : IOptionsMonitor<LoggerConfiguration>
    {
        public LoggerConfiguration CurrentValue => config;
        public LoggerConfiguration Get(string? name) => config;
        public IDisposable? OnChange(Action<LoggerConfiguration, string?> listener) => null;
    }
}
