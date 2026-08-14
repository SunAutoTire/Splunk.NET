using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SunAuto.Logging.Client;

/// <summary>
/// A custom JSON converter for serializing and deserializing Exception objects.
/// </summary>
public class ExceptionConverter : JsonConverter<Exception>
{
    /// <summary>
    /// Deepest <see cref="Exception.InnerException"/> chain that will be serialized. Chains longer
    /// than this are truncated with a marker.
    /// </summary>
    public const int MaxDepth = 32;

    /// <summary>
    /// Nesting level of the current serialization. Each property value is serialized in its own
    /// <see cref="JsonSerializer"/> operation, which resets that serializer's own depth counter,
    /// so <see cref="JsonSerializerOptions.MaxDepth"/> cannot bound this converter's recursion.
    /// Without a counter of our own, a long enough chain overflows the stack and takes the process
    /// down — an outcome far worse for a logging library than a truncated exception.
    /// Thread-static because serialization is synchronous and converters are shared across threads.
    /// </summary>
    [ThreadStatic]
    private static int _depth;

    /// <summary>
    /// Not supported. This converter is write-only; exceptions are serialized for transport to
    /// Splunk and are never read back.
    /// </summary>
    /// <param name="reader">The JSON reader from which the exception would be deserialized.</param>
    /// <param name="typeToConvert">The type of the object to convert.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>Never returns; always throws.</returns>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException($"{nameof(ExceptionConverter)} is write-only and cannot deserialize exceptions.");

    /// <summary>
    /// Serializes an Exception object to JSON, including its properties and values.
    /// </summary>
    /// <remarks>
    /// Each property is isolated: a getter that throws, or a value that cannot be serialized, is
    /// replaced by a diagnostic string for that property alone. The rest of the exception still
    /// makes it through, and the object is always well-formed JSON. That matters because the
    /// caller parses this output back into a <see cref="JsonDocument"/>, so emitting anything
    /// malformed here would discard the entire batch of log entries, not just this one value.
    /// </remarks>
    /// <param name="writer">The JSON writer to which the exception will be serialized.</param>
    /// <param name="value">The exception object to serialize.</param>
    /// <param name="options">Serialization options.</param>
    public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
    {
        if (_depth >= MaxDepth)
        {
            writer.WriteStartObject();
            writer.WriteString("ClassName", value.GetType().FullName);
            writer.WriteString("Truncated", $"Exception chain exceeded {MaxDepth} levels.");
            writer.WriteEndObject();
            return;
        }

        _depth++;

        try
        {
            WriteProperties(writer, value, options);
        }
        finally
        {
            _depth--;
        }
    }

    private static void WriteProperties(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var exceptionType = value.GetType();
        writer.WriteString("ClassName", exceptionType.FullName);

        var properties = exceptionType.GetProperties()
            .Where(e => e.PropertyType != typeof(Type))
            .Where(e => e.PropertyType.Namespace != typeof(MemberInfo).Namespace)
            .Where(e => e.GetIndexParameters().Length == 0);

        foreach (var property in properties)
        {
            string json;

            try
            {
                var propertyValue = property.GetValue(value, null);

                if (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull && propertyValue == null)
                    continue;

                // Serialize to a buffer first. Utf8JsonWriter cannot roll back a partial write, so
                // writing the property name before knowing the value is serializable risks leaving
                // a dangling name behind.
                json = JsonSerializer.Serialize(propertyValue, property.PropertyType, options);
            }
            catch (Exception ex)
            {
                writer.WriteString(property.Name, $"Cannot serialize value: {Unwrap(ex).Message}");
                continue;
            }

            writer.WritePropertyName(property.Name);
            writer.WriteRawValue(json);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Reflection wraps a throwing getter in a <see cref="TargetInvocationException"/>, whose own
    /// message says nothing useful. Report the underlying cause instead.
    /// </summary>
    private static Exception Unwrap(Exception ex)
        => ex is TargetInvocationException { InnerException: { } inner } ? inner : ex;
}
