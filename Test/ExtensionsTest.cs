using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Test;

public class ExtensionsTest
{
    [Theory(DisplayName = "ToLogLevel - Sorting & accuracy")]
    [InlineData("Trace", LogLevel.Trace)]
    [InlineData("Debug", LogLevel.Debug)]
    [InlineData("Information", LogLevel.Information)]
    [InlineData("Warning", LogLevel.Warning)]
    [InlineData("Error", LogLevel.Error)]
    [InlineData("Critical", LogLevel.Critical)]
    [InlineData("None", LogLevel.None)]
    public void Test0(string value, LogLevel logLevel)
    {
        var result = value.ToLogLevel();

        Assert.Equal(logLevel, result);
    }

    [Theory(DisplayName = "ToLogLevel - Exception handling")]
    [InlineData(null, typeof(ArgumentException), "Valid log level must be configured")]
    [InlineData("", typeof(ArgumentException), "Valid log level must be configured")]
    [InlineData("  \t", typeof(ArgumentException), "Valid log level must be configured")]
    [InlineData("Fart", typeof(ArgumentOutOfRangeException), "Valid log level must be configured")]
    [InlineData("Smell", typeof(ArgumentOutOfRangeException), "Valid log level must be configured")]
    public void Test1(string? value, Type exception, string messageStart)
    {
        var thrown = Assert.ThrowsAny<Exception>(() => value.ToLogLevel());

        Assert.IsType(exception, thrown);
        Assert.StartsWith(messageStart, thrown.Message);
    }

    [Theory(DisplayName = "ToEventId - Parsing")]
    [InlineData("5:", 5, null, null)]
    [InlineData("5:Smell",5,"Smell",null)]
    [InlineData("5", 5, null, null)]
    public void Test2(string value, int id, string? name, Type exception)
    {
        var result = value.ToEventId();

        Assert.Equal(id, result.Id);
        Assert.Equal(name, result.Name);
        //var thrown = Assert.ThrowsAny<Exception>(() => value.ToEventId());

        //Assert.IsType(exception, thrown);
    }

    [Theory(DisplayName = "ToEventId - Exception")]
    [InlineData(null, typeof(NullReferenceException))]
    [InlineData("", typeof(FormatException))]
    [InlineData("  \t", typeof(FormatException))]
    [InlineData("Fart", typeof(FormatException))]
    [InlineData("Smell:", typeof(FormatException))]
    public void Test3(string value, Type exception)
    {
        var thrown = Assert.ThrowsAny<Exception>(() => value.ToEventId());

        Assert.IsType(exception, thrown);
    }

}