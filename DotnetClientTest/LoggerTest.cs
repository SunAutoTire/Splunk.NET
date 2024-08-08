using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace SunAuto.Logging.Client.Test;

public class LoggerTest
{
    [Theory(DisplayName = "IsEnabled - Configuration")]
    [InlineData("Critical", LogLevel.Critical, true)]
    [InlineData("Debug", LogLevel.Debug, true)]
    [InlineData("Error", LogLevel.Error, true)]
    [InlineData("Information", LogLevel.Information, true)]
    [InlineData("None", LogLevel.None, true)]
    [InlineData("Trace", LogLevel.Trace, true)]
    [InlineData("Warning", LogLevel.Warning, true)]
    [InlineData("Information", LogLevel.Trace, false)]
    public void Test0(string level, LogLevel logLevel, bool condition)
    {
        var logger = GetLogger(level);

        Assert.Equal(condition, logger.IsEnabled(logLevel));

    }

    [Fact(DisplayName = "IsEnabled - Unconfigured")]
    public void Test1()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => GetLogger(String.Empty));

        Assert.IsType<InvalidOperationException>(exception);

        System.Diagnostics.Debug.WriteLine(exception.Message);
    }

    private static Logger GetLogger(string level) => new(GetConfiguration(level));

    private static IConfiguration GetConfiguration(string level)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);

        writer.Write(GetConfigurationJson(level));
        writer.Flush();
        stream.Position = 0;

        return new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
    }

    private static string GetConfigurationJson(string level)
    {
        var output = new StringBuilder();

        output.AppendLine("{");

        output.AppendLine("\"Logging\": {");
        output.AppendLine("\"LogLevel\": {");
        output.AppendLine("\"Default\": \"Trace\",");
        output.AppendLine("\"Microsoft\": \"Trace\"");
        output.AppendLine("},");
        output.AppendLine("\"SunAuto\": {");
        //output.AppendLine("\"Path\": \"..\\..\\..\\SunAuto.log\",");
        output.AppendLine("\"LogLevel\": {");
        if (!string.IsNullOrWhiteSpace(level)) output.AppendLine($"\"Default\": \"{level}\", ");
        output.AppendLine("\"Microsoft.Hosting\": \"Trace\"");
        output.AppendLine("}");
        output.AppendLine("},");
        output.AppendLine("\"Debug\": {");
        output.AppendLine("\"LogLevel\": {");
        output.AppendLine("\"Default\": \"Trace\", ");
        output.AppendLine("\"Microsoft.Hosting\": \"Trace\"");
        output.AppendLine("}");
        output.AppendLine("},");
        output.AppendLine("\"EventSource\": {");
        output.AppendLine("\"LogLevel\": {");
        output.AppendLine("\"Default\": \"Trace\"");
        output.AppendLine("}");
        output.AppendLine("}");
        output.AppendLine("}");
        output.AppendLine("}");

        return output.ToString();
    }
}