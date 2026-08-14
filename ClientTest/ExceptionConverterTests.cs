using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SunAuto.Logging.Client.Test;

/// <summary>
/// The sink parses this converter's output back into a <see cref="JsonDocument"/> before posting,
/// so malformed output here discards an entire batch of log entries. Every test in this class
/// asserts the result is well-formed JSON, whatever the exception throws at it.
/// </summary>
public class ExceptionConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new ExceptionConverter() },
    };

    private static JsonElement Serialize(Exception exception)
    {
        var json = JsonSerializer.Serialize(exception, Options);

        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            Assert.Fail($"Converter produced malformed JSON, which would discard the whole batch: {ex.Message}\nJSON: {json}");
            throw;
        }
    }

    [Fact]
    public void Writes_the_exception_type_and_message()
    {
        var result = Serialize(new InvalidOperationException("gateway timeout"));

        Assert.Equal("System.InvalidOperationException", result.GetProperty("ClassName").GetString());
        Assert.Equal("gateway timeout", result.GetProperty("Message").GetString());
    }

    [Fact]
    public void Writes_the_inner_exception()
    {
        var result = Serialize(new InvalidOperationException("outer", new ApplicationException("inner")));

        var inner = result.GetProperty("InnerException");
        Assert.Equal("System.ApplicationException", inner.GetProperty("ClassName").GetString());
        Assert.Equal("inner", inner.GetProperty("Message").GetString());
    }

    [Fact]
    public void A_throwing_getter_is_isolated_to_its_own_property()
    {
        var result = Serialize(new ThrowingGetterException());

        // The bad property is reported...
        Assert.Contains("Cannot serialize value", result.GetProperty("Detonate").GetString());

        // ...and everything else still made it through.
        Assert.Equal("message is fine", result.GetProperty("Message").GetString());
        Assert.True(result.TryGetProperty("HResult", out _));
    }

    [Fact]
    public void A_throwing_getter_reports_the_underlying_cause()
    {
        var result = Serialize(new ThrowingGetterException());

        // Reflection wraps the failure in TargetInvocationException, whose own message
        // ("Exception has been thrown by the target of an invocation.") is useless.
        Assert.Contains("this getter always throws", result.GetProperty("Detonate").GetString());
    }

    [Fact]
    public void A_deep_exception_chain_is_truncated_rather_than_overflowing_the_stack()
    {
        Exception exception = new InvalidOperationException("leaf");
        for (var i = 0; i < 20_000; i++)
            exception = new InvalidOperationException($"level {i}", exception);

        var result = Serialize(exception);

        Assert.Contains("Truncated", result.ToString());
    }

    [Fact]
    public void A_shallow_chain_is_not_truncated()
    {
        Exception exception = new InvalidOperationException("leaf");
        for (var i = 0; i < 5; i++)
            exception = new InvalidOperationException($"level {i}", exception);

        Assert.DoesNotContain("Truncated", Serialize(exception).ToString());
    }

    [Fact]
    public void A_self_referencing_exception_does_not_hang_or_overflow()
    {
        var exception = new SelfReferencingException("cyclic");
        exception.Self = exception;

        Assert.Equal("cyclic", Serialize(exception).GetProperty("Message").GetString());
    }

    [Fact]
    public void A_data_dictionary_with_an_unserializable_value_is_tolerated()
    {
        var exception = new InvalidOperationException("has data");
        exception.Data["key"] = new object();

        Assert.Equal("has data", Serialize(exception).GetProperty("Message").GetString());
    }

    [Fact]
    public void Read_is_not_supported()
    {
        var converter = new ExceptionConverter();

        Assert.Throws<NotSupportedException>(() =>
        {
            var reader = new Utf8JsonReader("{}"u8);
            converter.Read(ref reader, typeof(Exception), Options);
        });
    }

    /// <summary>
    /// The regression this class exists for: one hostile exception used to produce malformed JSON,
    /// which threw while the sink was building the request and discarded every entry in the batch.
    /// </summary>
    [Fact]
    public void A_hostile_exception_does_not_discard_the_rest_of_the_batch()
    {
        using var hec = new FakeHec();

        var provider = new LoggerProvider(new TestOptionsMonitor(new LoggerOptions
        {
            MinimumLevel = LogLevel.Trace,
            Splunk = new LoggerOptions.SplunkOptions
            {
                BaseUrl = hec.BaseUrl,
                Token = "test-token",
                Source = "test-sourcetype",
            },
        }));

        var log = provider.CreateLogger("Test");

        log.LogInformation("plain-before");
        log.LogError(new ThrowingGetterException(), "throwing-getter");
        log.LogError(DeepChain(20_000), "deep-chain");
        log.LogInformation("plain-after");

        provider.Dispose();

        var messages = hec.ParsedEvents()
            .Select(e => e.GetProperty("event").GetProperty("Message").GetString())
            .ToList();

        Assert.Contains("plain-before", messages);
        Assert.Contains("throwing-getter", messages);
        Assert.Contains("deep-chain", messages);
        Assert.Contains("plain-after", messages);
    }

    private static Exception DeepChain(int depth)
    {
        Exception exception = new InvalidOperationException("leaf");
        for (var i = 0; i < depth; i++)
            exception = new InvalidOperationException($"level {i}", exception);
        return exception;
    }

    private sealed class ThrowingGetterException : Exception
    {
        public override string Message => "message is fine";

        public string Detonate => throw new InvalidOperationException("this getter always throws");
    }

    private sealed class SelfReferencingException(string message) : Exception(message)
    {
        public SelfReferencingException? Self { get; set; }
    }
}
