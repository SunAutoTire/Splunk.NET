using Microsoft.Extensions.Logging;
using SunAuto.Logging.Client;

namespace SunAuto.Logging.Client.SplunkTest;

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

    const string Valid = "Valid log level must be configured";

    [Theory(DisplayName = "ToLogLevel - Exception handling")]
    [InlineData(null, typeof(ArgumentException), Valid)]
    [InlineData("", typeof(ArgumentException), Valid)]
    [InlineData("  \t", typeof(ArgumentException), Valid)]
    [InlineData("Fart", typeof(ArgumentOutOfRangeException), Valid)]
    [InlineData("Smell", typeof(ArgumentOutOfRangeException), Valid)]
    public void Test1(string? value, Type exception, string messageStart)
    {
        var thrown = Assert.ThrowsAny<Exception>(() => value.ToLogLevel());

        Assert.IsType(exception, thrown);
        Assert.StartsWith(messageStart, thrown.Message);
    }

    [Theory(DisplayName = "ToEventId - Parsing")]
    [InlineData("5:", 5, null)]
    [InlineData("5:Smell", 5, "Smell")]
    [InlineData("5", 5, null)]
    public void Test2(string value, int id, string? name)
    {
        var result = value.ToEventId();

        Assert.Equal(id, result.Id);
        Assert.Equal(name, result.Name);
        //var thrown = Assert.ThrowsAny<Exception>(() => value.ToEventId());

        //Assert.IsType(exception, thrown);
    }

    [Theory(DisplayName = "ToEventId - Exception")]
    //[InlineData(null, typeof(NullReferenceException))]
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